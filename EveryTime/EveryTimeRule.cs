using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EveryTime
{
    public class EveryTimeRule
    {
        private static int parseErrorCount = 0;
        private const int maxParseErrorCount = 10;

        private static int applyErrorCount = 0;
        private const int maxApplyErrorCount = 10;

        public HashSet<string> Causes = new HashSet<string>();
        public Dictionary<string, string> Effects = new Dictionary<string, string>();

        private static HashSet<string> possibleCauses = new HashSet<string>()
        {
                "Jump",
                "Dash",
                "Die",
                "ScreenChange",
                "Duck",
                "Refill",
                "Feather",
                "Bumper",
                "Spring",
                "DreamDash",
                "Land",
                "CollectBerry",
                "CollectKey",
                "CollectHeart",
                "UseKey",
                "NewRoom",
        };

        private static Dictionary<string, Action<Scene,string>> callbacks = new Dictionary<string, Action<Scene,string>>()
        {
            { "SpeedUp", (scene,args) => {
                if ( !float.TryParse( args, out float value ) )
                {
                    value = 0.01f;
                    LogParseError( "Could not parse arguments for SpeedUp. Using default value instead" );
                }
                EveryTimeModule.Session.TimeDilation += Math.Max( 0, value );
            } },
            { "SpeedDown", (scene,args) =>{
                if ( !float.TryParse( args, out float value ) )
                {
                    value = 0.01f;
                    LogParseError( "Could not parse arguments for SpeedDown. Using default value instead" );
                }
                EveryTimeModule.Session.TimeDilation *= 1 - Math.Max( 0, value );
            } },
            { "SpeedReset", (scene,args) =>{
                EveryTimeModule.Session.TimeDilation = 1f;
            } },
            { "ExtraHair", (scene,args) =>{
                if ( !int.TryParse( args, out int count ) )
                {
                    count = 1;
                    LogParseError( "Could not parse arguments for ExtraHair. Using default value instead" );
                }
                Player player = scene.Tracker.GetEntity<Player>();
                if ( player != null && player.Hair != null )
                {
                    player.Sprite.HairCount += count;
                    typeof( Player ).GetField( "startHairCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance ).SetValue( player, player.Sprite.HairCount );
                }
                EveryTimeModule.Session.ExtraHair = Math.Max( 0, EveryTimeModule.Session.ExtraHair + count );
            } },
            { "Hiccup", (scene,args) =>{
                string[] parts = args.Split( ',' );
                bool countParseFailed = false;
                bool delayParseFailed = false;
                if ( parts.Length <= 0 || !( countParseFailed = int.TryParse( parts[ 0 ], out int count ) ) )
                {
                    count = 1;
                    if ( countParseFailed)
                    {
                        LogParseError( "Could not parse first argument (count) for Hiccup. Using default value instead" );
                    }
                }
                if ( parts.Length <= 1 || !(delayParseFailed = float.TryParse( parts[ 0 ], out float delay ) ) )
                {
                    delay = 0.5f;
                    if ( delayParseFailed )
                    {
                        LogParseError( "Could not parse second argument (delay) for Hiccup. Using default value instead" );
                    }
                }
                Player player = scene.Tracker.GetEntity<Player>();
                if ( player != null && !player.Dead )
                {
                    player.Add( new Coroutine( HiccupCoroutine( player, count, delay ) ) );
                }
            } },
            { "BadelineChaser", (scene,args) =>{
                Player player = scene.Tracker.GetEntity<Player>();
                if ( !int.TryParse( args, out int value ) )
                {
                    value = 1;
                    LogParseError( "Could not parse arguments for BadelineChaser. Using default value instead" );
                }
                if ( player != null )
                {
                    if ( value > 0 )
                    {
                        int chaserCount = scene.Tracker.CountEntities<BadelineOldsite>() + scene.Tracker.CountEntities<EveryTimeCustomChaser>();
                        int added = 0;
                        while ( added < value )
                        {
                            if ( chaserCount >= 7 )
                            {
                                break;
	                        }
                            EveryTimeCustomChaser chaser = new EveryTimeCustomChaser( player.Position + Vector2.UnitY * 32, chaserCount++ );
                            scene.Add( chaser );
                            EveryTimeModule.Session.SpawnedBadelineChasers.Add( chaser );
                            added++;
                        }
                    }
                    else if ( value < 0 && EveryTimeModule.Session.ChaserCount > 0 )
                    {
                        int extraChasers = scene.Tracker.CountEntities<EveryTimeCustomChaser>();
                        int highestIndex = scene.Tracker.CountEntities<BadelineOldsite>() + extraChasers - 1;
                        int removed = 0;
                        int toRemove = -value;
                        while ( removed < toRemove && removed < extraChasers && EveryTimeModule.Session.SpawnedBadelineChasers.Count > 0 )
                        {
                            foreach ( EveryTimeCustomChaser chaser in EveryTimeModule.Session.SpawnedBadelineChasers )
                            {
                                var indexField = typeof( BadelineOldsite ).GetField( "index", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance );
                                int index = (int) indexField.GetValue( chaser );
                                if ( index == highestIndex )
                                {
                                    chaser.Add( new Coroutine( chaser.KillSelfRoutine() ) );
                                    break;
                                }
                            }
                            removed++;
                            highestIndex--;
                        }
                    }
                }
                EveryTimeModule.Session.ChaserCount = Math.Max( value + EveryTimeModule.Session.ChaserCount, 0 ); ;
            } },
            { "Oshiro", (scene,args) =>{
                if ( !int.TryParse( args, out int value ) )
                {
                    value = 1;
                    LogParseError( "Could not parse arguments for Oshiro. Using default value instead" );
                }
                Player player = scene.Tracker.GetEntity<Player>();
                EveryTimeModule.Session.OshiroCount = Math.Max( value + EveryTimeModule.Session.OshiroCount, 0 );
                if ( player != null )
                {
                    if ( value > 0 )
                    {
                        for (int i = 0; i < value; i++)
                        {
                            player.Add( new Coroutine( EveryTimeModule.OshiroSpawnRoutine( player, value ) ) );
                        }
                    }
                    else if ( value < 0 && EveryTimeModule.Session.OshiroCount > 0 )
                    {
                        int removed = 0;
                        int index = EveryTimeModule.Session.SpawnedOshiros.Count - 1;
                        while (index >= 0 && removed > value )
                        {
                            AngryOshiro oshiro = EveryTimeModule.Session.SpawnedOshiros[ index ];
                            EveryTimeModule.Session.SpawnedOshiros.RemoveAt(index);
                            oshiro.RemoveSelf();
                            index = EveryTimeModule.Session.SpawnedOshiros.Count - 1;
                            removed--;
                        }
                        Distort.Anxiety = 0f;
                    }
                }
            } },
            { "Anxiety", (scene,args) =>{
                if ( !float.TryParse( args, out float value ) )
                {
                    value = 0.05f;
                    LogParseError( "Could not parse arguments for Anxiety. Using default value instead" );
                }
                EveryTimeModule.Session.AnxietyBonus = Math.Max( 0, EveryTimeModule.Session.AnxietyBonus + value );
            } }
        };

        internal static void CheckForErrors()
        {
            int errorCount = 0;
            foreach ( EveryTimeRule rule in EveryTimeModule.Settings.Rules )
            {
                foreach ( string cause in rule.Causes )
                {
                    if ( !possibleCauses.Contains(cause) )
                    {
                        Logger.Log( "EveryTimeRule", $"Found a rule with unrecognized cause: {cause}!" );
                        errorCount++;
                    }
                }
                foreach ( string effectName in rule.Effects.Keys )
                {
                    if ( !callbacks.ContainsKey( effectName ) )
                    {
                        Logger.Log( "EveryTimeRule", $"Found a rule with unrecognized effect: {effectName}!" );
                        errorCount++;
                    }
                }
            }
            if ( errorCount > 0 )
            {
                Logger.Log( "EveryTimeRule", $"Check {UserIO.GetSaveFilePath( $"modsettings-{EveryTimeModule.Instance.Metadata.Name}" )} to fix these issues." );
            }
        }

        public void Apply( Scene scene )
        {
            if ( Effects.Count == 0 )
            {
                LogApplyError( $"Rule with causes: {string.Join(",", Causes.ToArray())} had no effects associated with it" );
                return;
            }
            foreach ( KeyValuePair<string,string> effectArgPair in Effects )
            {
                if ( callbacks.TryGetValue( effectArgPair.Key, out Action<Scene,string> callback ) )
                {
                    if ( callback != null )
                    {
                        callback.Invoke( scene, effectArgPair.Value );
                    }
                    else
                    {
                        LogApplyError( $"Callback for Effect: {effectArgPair.Key} was registered with a null value. Make sure you implemented the callback correctly, or if it is not you who made the extension mod, notify the mod's creator!" );
                    }
                }
                else
                {
                    LogApplyError( $"There is no registered action for Effect: {effectArgPair.Key}" );
                }
            }
        }

        public static void RegisterEffectCallback( string effectName, Action<Scene,string> callback )
        {
            bool isNew = !callbacks.ContainsKey( effectName );
            callbacks.Add( effectName, callback );
            if ( isNew )
            {
                Logger.Log( "EveryTimeRule", $"Registered new effect with name: {effectName}" );
            }
            else
            {
                Logger.Log( "EveryTimeRule", $"Overwrote existing effect with name: {effectName}" );
            }
        }

        public static void RegisterCause( string causeName )
        {
            bool isNew = !possibleCauses.Contains( causeName );
            possibleCauses.Add( causeName );
            if ( isNew )
            {
                Logger.Log( "EveryTimeRule", $"Registered new cause with name: {causeName}" );
            }
            else
            {
                Logger.Log( "EveryTimeRule", $"Tried to register new cause with name: {causeName}, but it was already registered" );
            }
        }

        private static void LogParseError( string msg)
        {
            if ( parseErrorCount < maxParseErrorCount )
            {
                Logger.Log( "EveryTimeRule", msg );
                parseErrorCount++;
                if ( parseErrorCount >= maxParseErrorCount )
                {
                    Logger.Log( "EveryTimeRule", $"Reached maximum count ({maxParseErrorCount}) of parsing error log messages. No more parse errors will be logged in order to not clog the log." );
                }
            }
        }

        private static void LogApplyError( string msg )
        {
            if ( applyErrorCount < maxApplyErrorCount )
            {
                Logger.Log( "EveryTimeRule", msg );
                parseErrorCount++;
                if ( applyErrorCount >= maxApplyErrorCount )
                {
                    Logger.Log( "EveryTimeRule", $"Reached maximum count ({maxApplyErrorCount}) of rule apply error log messages. No more rule apply errors will be logged in order to not clog the log." );
                }
            }
        }

        private static IEnumerator HiccupCoroutine( Player player, int count, float delay )
        {
            while ( count > 0)
            {
                yield return delay;
                player.HiccupJump();
                count--;
            }
        }
    }
}
