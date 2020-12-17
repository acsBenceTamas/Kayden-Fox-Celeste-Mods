using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace EtherHelper.Entities
{
    [CustomEntity( "EtherHelper/CurvedWindPath" )]
    class CurvedWindPath : Entity
    {
        private const int pointsPerSegment = 10;
        private const float _stepModifier = 10f;
        private const float _frequencyModifier = 200.0f;
        private const float _windUpTime = 0.5f;
        private const float _windDownTime = 0.2f;

        private readonly int _tunnelWidth;
        private readonly float _strength;
        private readonly float _alpha;

        private Vector2[] _points;
        private Vector2[] _directions;
        private Vector2[] _outerPoints;
        private List<Bubble> _bubbles = new List<Bubble>();
        private Dictionary<WindMover, float> _componentPercentages = new Dictionary<WindMover, float>();

        private float _bubbleSpawnTimer;
        private readonly float _bubbleFrequency;
        private readonly Random _random = new Random();

        public CurvedWindPath( EntityData data, Vector2 offset ) : this( data.NodesWithPosition(offset), data.Int( "tunnelWidth", 64 ), data.Float( "strength", 100.0f ), data.Float( "alpha", 0.5f ) )
        { }

        public CurvedWindPath( Vector2[] nodesWithPosition, int tunnelWidth, float strength, float alpha )
        {
            _tunnelWidth = tunnelWidth;
            _strength = strength;
            _alpha = alpha;
            _bubbleFrequency = _frequencyModifier / _strength / _tunnelWidth;

            GenerateSplinePoints( nodesWithPosition, _alpha );
            GenerateOuterPoints( nodesWithPosition );
            GenerateStartingBubbles();
        }

        public override void Update()
        {
            base.Update();

            for ( int i = _bubbles.Count - 1; i >= 0; i-- )
            {
                Bubble bubble = _bubbles[ i ];

                bubble.StepForward( _strength );

                if ( bubble.Percentage > 1.0f )
                {
                    _bubbles.RemoveAt( i );
                    continue;
                }
            }

            _bubbleSpawnTimer += Engine.DeltaTime;

            while ( _bubbleSpawnTimer > _bubbleFrequency )
            {
                _bubbleSpawnTimer -= _bubbleFrequency;
                _bubbles.Add( new Bubble( _outerPoints, _alpha, _random.NextFloat(), _random.NextFloat( 2.0f ) + 3.0f ) );
            }

            foreach ( WindMover component in Scene.Tracker.GetComponents<WindMover>() )
            {
                GetWindingNumber( component.Entity.Center, new Vector2[] { component.Entity.TopLeft, component.Entity.TopRight, component.Entity.BottomRight, component.Entity.BottomLeft } );

                if ( IsInside( component.Entity ) )
                {
                    if ( _componentPercentages.ContainsKey( component ) )
                    {
                        _componentPercentages[ component ] = Calc.Approach( _componentPercentages[ component ], 1f, Engine.DeltaTime / _windUpTime );
                    }
                    else
                    {
                        _componentPercentages.Add( component, 0f );
                    }
                }
                else
                {
                    if ( _componentPercentages.ContainsKey( component ) )
                    {
                        _componentPercentages[ component ] = Calc.Approach( _componentPercentages[ component ], 0.0f, Engine.DeltaTime / _windDownTime );
                        if ( _componentPercentages[ component ] == 0f )
                        {
                            _componentPercentages.Remove( component );
                        }
                    }
                }
            }
            foreach ( WindMover component in _componentPercentages.Keys )
            {
                if ( component != null && component.Entity != null && component.Entity.Scene != null )
                {
                    Vector2 windSpeed = GetWindSpeed( component.Entity.Center );
                    component.Move( windSpeed * Engine.DeltaTime * Ease.CubeInOut( _componentPercentages[ component ] ) );
                }
            }
            base.Update();
        }

        public override void Render()
        {
            base.Render();

            for ( int i = _bubbles.Count - 1; i >= 0; i-- )
            {
                Bubble bubble = _bubbles[ i ];

                Color color = Color.White;
                Draw.Circle( bubble.Position, bubble.Size, color, 4 );
            }

            foreach ( WindMover component in Scene.Tracker.GetComponents<WindMover>() )
            {
                float percentage;
                if ( _componentPercentages.TryGetValue( component, out percentage ) )
                {
                    Draw.LineAngle( component.Entity.Center, GetWindSpeed( component.Entity.Center ).Angle(), percentage * _strength, Color.Purple );
                }
            }
        }

        public override void DebugRender( Camera camera )
        {
            base.DebugRender( camera );

            foreach ( Vector2 segmentNode in _points )
            {
                Draw.Circle( segmentNode, 4, Color.Red, 4 );
            }

            foreach ( Vector2 segmentNode in _outerPoints )
            {
                Draw.Circle( segmentNode, 4, Color.Blue, 4 );
            }

            for ( int i = 0; i < _directions.Length; i++ )
            {
                Draw.LineAngle( _points[ i ], _directions[ i ].Angle(), 10, Color.Green );
            }

            for ( int i = 0; i < _outerPoints.Length; i++ )
            {
                if ( i < _outerPoints.Length - 1 )
                {
                    Draw.Line( _outerPoints[ i ], _outerPoints[ i + 1 ], Color.Blue );
                }
                else
                {
                    Draw.Line( _outerPoints[ i ], _outerPoints[ 0 ], Color.Blue );
                }
                if ( i < _outerPoints.Length / 2 )
                {
                    Draw.Line( _outerPoints[ i ], _outerPoints[ _outerPoints.Length - 1 - i ], Color.Yellow );
                }
            }
        }

        private void GenerateStartingBubbles()
        {
            float pos = 0.0f;
            float lineLength = 0.0f;
            float stepSize = _strength / _stepModifier / _bubbleFrequency;

            for ( int i = 0; i < _points.Length - 1; i++ )
            {
                lineLength += ( _points[ i + 1 ] - _points[ i ] ).Length();
            }

            while ( pos < lineLength )
            {
                _bubbles.Add( new Bubble( _outerPoints, _alpha, _random.NextFloat(), _random.NextFloat(2.0f) + 3.0f, pos/lineLength ) );
                pos += stepSize;
            }
        }

        private void GenerateOuterPoints( Vector2[] centerPoints )
        {
            int segmentDivision = 3;
            int halfLength = centerPoints.Length + ( centerPoints.Length - 1 ) * ( segmentDivision - 1 );
            _outerPoints = new Vector2[ halfLength * 2 ];
            float stepSize = (float)pointsPerSegment / (float)segmentDivision;

            for ( int i = 0; i < halfLength; i++ )
            {
                Vector2 perpendicular;
                int targetIndex = (int)Math.Round( i * stepSize );

                if ( i == halfLength - 1 )
                {
                    perpendicular = _directions[ _directions.Length - 1];
                }
                else if ( i == 0 )
                {
                    perpendicular = _directions[ 0 ];
                }
                else
                {
                    perpendicular = _directions[ targetIndex - 1 ] + _directions[ targetIndex ] + _directions[ targetIndex + 1 ];
                }
                perpendicular = perpendicular.Rotate( (float)Math.PI * 0.5f );
                perpendicular.Normalize();
                _outerPoints[ i ] = _points[ targetIndex ] + perpendicular * _tunnelWidth * 0.5f;
                _outerPoints[ _outerPoints.Length - 1 - i ] = _points[ targetIndex ] - perpendicular * _tunnelWidth * 0.5f;
            }
        }

        private void GenerateSplinePoints( Vector2[] centerPoints, float alpha )
        {
            _points = new Vector2[ ( centerPoints.Length - 1 ) * pointsPerSegment + 1 ];
            _directions = new Vector2[ _points.Length - 1 ];

            for ( int i = 0; i < centerPoints.Length - 1; i++ )
            {
                GenerateSplineSegment( centerPoints, i, alpha, ref _points );
            }

            _points[ _points.Length - 1 ] = centerPoints[ centerPoints.Length - 1 ];

            for ( int i = 0; i < _points.Length - 1; i++ )
            {
                _directions[ i ] = ( _points[ i + 1 ] - _points[ i ] ).SafeNormalize();
            }
        }

        private static void GenerateSplineSegment( Vector2[] centerPoints, int i, float alpha, ref Vector2[] points )
        {
            Vector2[] pointsToFit = new Vector2[ 4 ];
            if ( i == 0 ) // first segment has no previous point
            {
                pointsToFit[ 0 ] = centerPoints[ i ] + ( centerPoints[ i ] - centerPoints[ i + 1 ] ).SafeNormalize(); // we extrapolate the edge points
                pointsToFit[ 1 ] = centerPoints[ i ];
                pointsToFit[ 2 ] = centerPoints[ i + 1 ];
                pointsToFit[ 3 ] = centerPoints[ i + 2 ];
            }
            else if ( i == centerPoints.Length - 2 ) // last segment has no point afterwards
            {
                pointsToFit[ 0 ] = centerPoints[ i - 1 ];
                pointsToFit[ 1 ] = centerPoints[ i ];
                pointsToFit[ 2 ] = centerPoints[ i + 1 ];
                pointsToFit[ 3 ] = centerPoints[ i + 1 ] + ( centerPoints[ i + 1 ] - centerPoints[ i ] ).SafeNormalize(); // we extrapolate the edge points
            }
            else
            {
                pointsToFit[ 0 ] = centerPoints[ i - 1 ];
                pointsToFit[ 1 ] = centerPoints[ i ];
                pointsToFit[ 2 ] = centerPoints[ i + 1 ];
                pointsToFit[ 3 ] = centerPoints[ i + 2 ];
            }

            Vector2[] intermediatePoints = CatmulRom( pointsToFit, alpha );

            for ( int j = 0; j < pointsPerSegment; j++ )
            {
                points[ i * ( pointsPerSegment ) + j ] = intermediatePoints[ j ];
            }
        }

        private Vector2 GetWindSpeed( Vector2 center )
        {
            Vector2[] closestDirs = GetClosestDirs( center );
            Vector2 speed = ( closestDirs[ 0 ] + closestDirs[ 1 ] + closestDirs[ 2 ] ).SafeNormalize() * _strength;
            if ( Math.Abs(speed.X) < 5f )
            {
                speed.X = 0;
            }
            return speed;
        }

        private bool IsInside( Entity entity )
        {
            Vector2[] pointsToCheck = new Vector2[] { entity.Center, entity.TopLeft, entity.TopRight, entity.BottomLeft, entity.BottomRight };
            foreach ( Vector2 point in pointsToCheck )
            {
                if ( GetWindingNumber( point, _outerPoints ) != 0 )
                {
                    return true;
                }
            }
            return false;
        }

        private Vector2[] GetClosestDirs( Vector2 center )
        {
            List< Tuple< Vector2, int > > orderedPoints = new List<Tuple<Vector2, int>>();

            for ( int i = 0; i < _points.Length - 1; i++ )
            {
                orderedPoints.Add( new Tuple<Vector2, int>( _points[ i ], i ) );
            }

            orderedPoints.Sort( (one, other) => Math.Sign(( ( center - one.Item1 ).LengthSquared() - ( center - other.Item1 ).LengthSquared() ) ) );
            Vector2[] dirs = new Vector2[ 3 ];

            for ( int i = 0; i < 3; i++ )
            {
                dirs[ i ] = _directions[ orderedPoints[i].Item2 ];
            }

            return dirs;
        }

        private static Vector2[] CatmulRom( Vector2[] points, float alpha ) // Generates intermediate spline points
		{
			Vector2[] newPoints = new Vector2[ pointsPerSegment ]; 

			Vector2 p0 = points[ 0 ];
			Vector2 p1 = points[ 1 ];
			Vector2 p2 = points[ 2 ];
			Vector2 p3 = points[ 3 ];

			float t0 = 0.0f;
			float t1 = GetT( t0, p0, p1, alpha );
			float t2 = GetT( t1, p1, p2, alpha );
			float t3 = GetT( t2, p2, p3, alpha );

			int i = 0;

			for ( float t = t1; t < t2; t += ( ( t2 - t1 ) / (float)pointsPerSegment ) )
			{
                if ( i == pointsPerSegment )
                {
                    break;
                }
				Vector2 A1 = ( t1 - t ) / ( t1 - t0 ) * p0 + ( t - t0 ) / ( t1 - t0 ) * p1;
				Vector2 A2 = ( t2 - t ) / ( t2 - t1 ) * p1 + ( t - t1 ) / ( t2 - t1 ) * p2;
				Vector2 A3 = ( t3 - t ) / ( t3 - t2 ) * p2 + ( t - t2 ) / ( t3 - t2 ) * p3;

				Vector2 B1 = ( t2 - t ) / ( t2 - t0 ) * A1 + ( t - t0 ) / ( t2 - t0 ) * A2;
				Vector2 B2 = ( t3 - t ) / ( t3 - t1 ) * A2 + ( t - t1 ) / ( t3 - t1 ) * A3;

				Vector2 C = ( t2 - t ) / ( t2 - t1 ) * B1 + ( t - t1 ) / ( t2 - t1 ) * B2;

				newPoints[ i ] = C;
                i++;
			}

            return newPoints;
		}

        private float IsLeft( Vector2 vertex1, Vector2 vertex2, Vector2 point )
        {
            return ( ( vertex2.X - vertex1.X ) * ( point.Y - vertex1.Y )
                    - ( point.X - vertex1.X ) * ( vertex2.Y - vertex1.Y ) );
        }

        private int GetWindingNumber( Vector2 point, Vector2[] edges )
        {
            int windingNumber = 0;

            for ( int i = 0; i < edges.Length; i++ )
            {
                int iPlusOne = i + 1;
                if ( iPlusOne == edges.Length )
                {
                    iPlusOne = 0;
                }

                if ( edges[ i ].Y <= point.Y )
                {
                    if ( edges[ iPlusOne ].Y > point.Y )
                        if ( IsLeft( edges[ i ], edges[ iPlusOne ], point ) > 0 )
                            ++windingNumber;
                }
                else
                {
                    if ( edges[ iPlusOne ].Y <= point.Y )
                        if ( IsLeft( edges[ i ], edges[ iPlusOne ], point ) < 0 )
                            --windingNumber;
                }
            }

            return windingNumber;
        }

        private static float GetT( float t, Vector2 p0, Vector2 p1, float alpha )
        {
            float a = (float)(Math.Pow( ( p1.X - p0.X ), 2.0 ) + Math.Pow( ( p1.Y - p0.Y ), 2.0f ));
            float b = (float)Math.Pow( a, alpha * 0.5f );

            return ( b + t );
        }

        private class Bubble
        {
            public readonly float Size;
            public readonly Vector2[] Path;
            public readonly float[] SegmentStarts;
            public readonly float RouteLength;
            public float ForwardPosition;

            public float Percentage { get { return ForwardPosition / RouteLength; } set { ForwardPosition = RouteLength * value; CalculateSegnentIndex(); } }
            public int SegmentIndex { get; private set; }
            public float LocalFordardPosition { get { return ForwardPosition - SegmentStarts[ SegmentIndex ]; } }
            public Vector2 Position { get { return Path[ SegmentIndex ] + ( Path[ SegmentIndex + 1 ] - Path[ SegmentIndex ] ).SafeNormalize() * LocalFordardPosition; } }

            public Bubble( Vector2[] outerPoints, float alpha, float sidePercent, float size, float forwardPercent = 0.0f )
            {
                Size = size;
                Path = new Vector2[ ( outerPoints.Length / 2 - 1 ) * pointsPerSegment + 1 ];

                Vector2[] cornerPoints = new Vector2[ outerPoints.Length / 2 ];
                for ( int i = 0; i < outerPoints.Length / 2; i++ )
                {
                    cornerPoints[ i ] = outerPoints[ i ] - ( outerPoints[ i ] - outerPoints[ outerPoints.Length - 1 - i ] ) * sidePercent;
                }

                for ( int i = 0; i < cornerPoints.Length - 1; i++ )
                {
                    GenerateSplineSegment( cornerPoints, i, alpha, ref Path );
                }

                Path[ Path.Length - 1 ] = cornerPoints[ cornerPoints.Length - 1 ];

                RouteLength = 0;
                SegmentStarts = new float[ Path.Length - 1];

                for ( int i = 0; i < Path.Length - 1; i++ )
                {
                    SegmentStarts[ i ] = RouteLength;
                    float segmentLength = (Path[ i + 1 ] - Path[ i ]).Length();
                    RouteLength += segmentLength;
                }

                Percentage = forwardPercent;
            }

            private void CalculateSegnentIndex()
            {
                float pos = ForwardPosition;
                for ( int i = 0; i < SegmentStarts.Length; i++ )
                {
                    if ( SegmentStarts[i] >= pos )
                    {
                        SegmentIndex = i;
                        return;
                    }
                }
            }

            public void StepForward( float strength )
            {
                ForwardPosition += strength / _stepModifier;
                while ( SegmentIndex != SegmentStarts.Length - 1 && ForwardPosition > SegmentStarts[ SegmentIndex + 1 ] )
                {
                    SegmentIndex++;
                }
            }
        }
    }
}
