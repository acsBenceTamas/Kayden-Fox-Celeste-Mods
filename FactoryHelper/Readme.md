# Factory Helper

## A Celeste mod by Kayden Fox

This mod was mainly created for **[Shrouded Thoughts](https://gamebanana.com/maps/206930)**, a post Farewell story chapter I made.
The main gimmick of this mod is a whole bunch of industrial entities that can be toggled on and off through various means.
This repo contains the source code and most assets used. For the actual download, see the [downloads](#downloads) section
of this README. The mod and all levels using it require the Everest mod loader. You can find a link to it in the [dependencies](#dependencies) section.

**IMPORTANT!** If you ever notice any bugs with this mod or have feedback on any of the levels
linked in this document, please contact me on Discord at `Kayden Fox#4830` or in the modding
section of the official Celeste Discord server!

### Table of contents
- [Features](#features)
- [Downloads](#downloads)
- [Dependencies](#dependencies)
- [Special thanks](#thanks)

### <a name=features>Features
#### __Piston__
This entity consists of 3 distinct parts: the head, the base and the body. The base is static, the head moves between a 
start and end node point and the body extends and contracts to fit the space between the base and the head. The move 
and wait times of this entity can be set to any number. They provide momentum to the player. There is an option to
make the piston's body heated which makes it so that a player standing on it or grabbing it evaporates. You can attach
entities like spikes or spinners to the head, but not the body. In Ahorn the you can position the start and end positions,
a blue arrow pointing from the start to the end position. The piston can face any of the four cardinal directions.

- **moveTime**: The time it takes for the piston head to travel between its start and end position in seconds.
- **pauseTime**: The time the piston head waits in place after movement is done before moving again in seconds.
- **initialDelay**: The time it takes before the piston first starts moving in seconds. If the piston is active
when the player enters the room, the value is instead considered as `initialDelay % ( 2 * ( moveTime + pauseTime )`
so that they act more like they have been going for a while.
- **startActive**: Boolean value that determines what the default state of the piston is. If `true` the piston will 
initially be moving. Defaults to `true`.
- **heated**: Boolean value that determines whether the pipe is heated or not. Defaults to `false`.
- **activationId**: String value of the piston's Activation Id. When a factory activator with this Id is turned on
the piston will toggle state.

#### __Boom Box__
This 3x3 tile entity serves as a launcher for the player. When active, if the player grabs it or stands on it, it will
launch the player after 0.5 seconds, after which it goes into a dormant state for 2.0 seconds. After activated, it takes
1.5 seconds before it can launch the player for the first time.

- **activationId**: String value of the boom box's Activation Id. When a factory activator with this Id is turned on
the boom box will start its activation or deactivation sequence after the initial delay is done.
- **initialDelay**: The time it takes before the boom box starts turning on or off after a factory activator with its
Activation Id is turned on. If the activator is on when the player enters the room this field is ignored and it will
be active from the start.
- **startActive**: Boolean value that dtermines what the default state of the boom box is. If `true` the boom box will
start ready to launch the player. Defaults to `false`.

#### __Dash Negator__
This entity has a minimum size and size increment of 2 tiles (16 units) wide and 4 tiles (32 units) tall. The turrets on
top are thin solids. The sprite size is not representative of the hitbox size. If the player initiates a dash inside
the red area cast by the turrets they will be shot by a laser and immediately killed.

- **activationId**: String value of the dash negator's Activation Id. When a factory activator with this Id is turned on
the dash negator will swap its active state immediately.
- **startActive**: Bollean value that determines the default state of the dash negator. If `true` the dash negator
will start with its negator field active. Defautls to `true`.

#### __Wind Tunnel__
Localized wind that can blow in any of the four cardinal directions with arbitrary strength. Visually the stronger the wind
the longer the particle effects of the tunnel will be and the faster they move. When entered, the wind strength will affect
the player gradually over 0.5 seconds and will stop affecting the player after leaving over 0.2 seconds. Works on any entity
with a WindMover component! **NOTE:** when moving towards walls with upwards wind you slide up them if the wind is strong enough.

- **direction**: String value that determines the direction the wind blows inside the wind tunnel.
- **activationId**: String value of the wind tunnel's Activation Id. When a factory activator with this Id is turned on
the wind tunnel will swap its speed between its full speed and zero over 1.5 seconds when speeding up and 1.0 seconds
when slowing down.
- **strength**: This value determines the wind strength. Experiment with these values to find the strength you desire.
Some familiar values from the base game are:
	* Weak horizontal: 400
	* Strong horizontal: 800
	* Crazy horizontal: 1200
	* Summit updraft: 300
	* Summit downdraft: 400
- **startActive**: Bollean value that determines the default state of the wind tunnel. If `true` the wind tunnel 
will be active on start. Defaults to `false`.

#### __Fan__
Purely aesthetic solid entity. Minimum size is 2x3 or 3x2 depending on orientation. Comes in Horizontal and Vertical variations.
Used in conjunction with Wind Tunnels.

- **startActive**: Bollean value that determines the default state of the fan. If `true` the fan will be active on start.
Defaults to `false`.
- **activationId**: String value of the fan's Activation Id. When a factory activator with this Id is turned on the fan will
start speeding up over 1.5 seconds or slowing down over 1.0 seconds, depending on the initial state.

#### __Electrified Wall__
Generally acts like spikes, but can be toggled by factory activators. Unlike regular spikes they are omnidirectional, meaning
they will kill the player regardless of which direction they are coming from.

- **activationId**: String value of the electrified wall's Activation Id. When a factory activator with this Id is turned on,
the wall will immediatels swap its state.
- **startActive**: Boolean value that determines the default state of the electrified wall. If `true` the wall will start active.
Defaults to `true`.

#### __Conveyor Belt__
A horizontal conveyor belt that moves entities with the ConveyorMover Component (currently the Player and ThrowBox classes). It 
can move these entities left or right, depending on the state of the belt.

- **activationId**: String value of the conveyor's Activation Id. When a factory activator with this Id is turned on, the 
direction of the conveyor belt is swapped immediately.
- **startLeft**: Boolean value that determines the initial direction of the belt. When `true` the conveyor belt will move 
towards the left. Defautls to `true`.

#### __Crate__
A holdable crate made of either wood or metal. Metal crates are only destroyed if they hit spinners or debris. Wooden crates
are destroyed when they hit the ground or a wall too fast. Crates can have two additional properties.
- **Special**: A crate with this property will return the player to where they first picked it up after either the player dies
or the crate is destroyed. Used for when losing a crate would make a later screen impossible. **Note:** The crate can still be left behind
however!
- **Crucial**: When a crate with this property is destroyed they also kill the player. Used for screens where losing the crate
makes the rest of the room impossible.

- **isMetal**: Boolean value that determines whther the crate is metallic or not. If set to `false` it will be wooden. Defaults to `false`.
- **tutorial**: Boolean value that determines whether the crate should show the pickup and put down tutorials.
- **isSpecia**: Boolean value that determines whether the crate is Special.
- **isCrucial**: Boolean value that determines whether the crate is Crucial.

#### __Crate Spawner__
This entity is invisible and spawns crates. It has multiple settings for how it behaves. It can spawn a specific type of crate or random.
It can spawn a set finite number of crates or infinite amounts of them. It is recommended that you never actually spawn an infinite number
of them however! The spawned crate can also have the tutorial messages. The direction from which the crate is spawned can be also set.
When it is set to spawn from top, it will create the crates just outside of the room's top bounds. Otherwise it tries to spawn them on the
nearest side just outside of the room bounds. In conjunction with Conveyor Belts that extend out of the room you can bring these side spawned
crates inside the room.

- **delay**: The amount if time before a new crate is spawned after the previous one in seconds.
- **maximum**: The maximum number of crates that can be spawned by a spawner. If one of the already spawned crates are destroyed, it no longer
counts towards this maximum.
- **activationId**: String value of the crate spawner's Activation Id. When a factory activator with this Id is turned on, the state of
the spawner is immediately switched.
- **isMetal**: Boolean value that determines whether the spawned crates will be metallic. If set to `false` they will be wooden. If `isRandom` is
`true` this field is ignored. Defaults to `false`.
- **isRandom**: Boolean value that determines whether the spawned crates should have a random material. If set to `true` the `isMetal` value will be
ignored. Defaults to `false`.
- **fromTop**: Boolean value that determines the direction where crates are spawned from. If `true` they will spawn on top, otherwise they will
spawn at the nearest side. Defautls to `true`.
- **tutorial**: Boolean value that determines whether spawned crates should show the tutorial messages. Defautls to `false`.
- **startActive**: Boolean value that determines whether the spawner should spawn crates by default. Defaults to `true`.

#### __Dash Fuxe Box__
These fuse boxes are factory activators. When you break one by dashing into it or throwing a holdable at it (like Crates), they bust and
will activate all Activation Ids they are set up with. They refill the player's dash when busted directly by the player. When all Activation
Ids are already activated when you enter a room with a dash fuse box it will start busted, whether it was actually busted by the player or not.

- **activationIds**: A comma separated set of string Ids for all the Activation Ids this box should activate.
- **persistent**: Boolean value that determines whether the Ids not already persistent that are governed by this box should become
persistent, meaning they will be active even after leaving the room.

#### __Battery Box & Battery__
A battery box is a non-solid entity that, when provided with a battery will activate all Activation Ids that are set for it. Batteries
act the same way as keys do: they persist if the player dies and they are consumed upon use. The player needs to be in direct line of sight
with the box's center to insert a battery.

- **activationIds**: A comma separated set of string Ids for all the Activation Ids this box should activate.

#### __Pressure Plate__
A button on the floor that, while pressed by an Actor (i.e: Player, Crate, Theo), will keep all Activation Ids that are set for it active. Once
there is no Actor on top of the button anymore all Ids not set by other factory activators will deactivate immediately.

- **activationIds**: A comma separated set of string Ids for all the Activation Ids this pressure plate should activate.

#### __Machine Heart__
A solid entity that, when dashed into twice, will crumble all Dash Blocks in the room and summon a Steam Wall after 2 seconds at the left
edge of the screen. It also initiates the Escape music from Shrouded Thoughts.

#### __Steam Wall__
Not a directly placable entity. It is spawned by a Spawn Steam Wall Trigger or a Machine Heart. It will move towards the right side of the screen
at a steady pace, killing the player on touch and affecting all factory activated entities (aside from Power Lines). It disables all activated
entities except Boom Boxes, which explode immediately, even if they were not previously active. When there is no Steam Wall present in the room,
a Spawn Steam Wall Trigger will spawn a new one on the left edge of the camera. If there already is one, it will advance the wall to the left edge
of the screen. A single trigger can only activate once, but multiple can be used to advance the steam faster than its default speed.

#### __Power Line__
These are used to indicate connections between factory activators (Battery Boxes, Fuse Boxes and Pressure Plates) and activated entities. They can
only be placed in perpendicular lines. If placed in any other way, they will attempt to fix their positions into a state where they will be perpendicular
probably causing unintended corner positions. When two power lines with the same Activation Id and initial activation state share a node coordinate, they
merge at that coordinate, allowing for forked power lines. Colors are used to differentiate power lines with different Ids close to each other, but are
optional.

- **activationId**: String value of the power line's Activation Id. When a factory activator with this Id is turned on, the state of
the power line is immediately switched.
- **colorCode**: Hex color code (example: ff0000 is red) for the power line's power color, in both active and inactive state. Inactive is a dimmer version
of this hex color.
- **startActive**: Boolean value that determines whether the power line should be lit up by default or not.
- **initialDelay**: The time it takes for the power line to switch states after it is activated. Is ignored when entering the room when the Id is
already active.

#### __Rusty Lamp__
Light source with different strobe patterns that can be activated or deactivated by factory activators.

- **activationId**: String value of the lamp's Activation Id. When a factory activator with this Id is turned on, the lamp will switch states after the
initial delay.
- **strobePattern**: The strobe pattern the lamp will use. The possible values are:
	* None - the lamp will turn on and off immediately. No additional effects.
	* FlickerOn - the lamp will flicker on and off. No additional effects.
	* LightFlicker - the lamp will flicker on and off. Periodically it lightly flickers for a short while at random.
	* TurnOffFlickerOn - the lamp will flicker on and off. Periodically it will flicker off for a short duration then flicker back on at random.
- **initialDelay**: The amount of time it takes for the lamp to turn on after it is activated in seconds. This is ignored if you enter a room when its Id is 
already active.
- **startActive**: Boolean value that determines whether the lamp should start on or off. Defaults to `false`.

#### __Killer Debris__
An entity that visually and mechanically acts like spinners, made of scrap metal. They come in two varieties: Bronze (brown) and Silver (grey).
Just like spinners they can be attached to solids.

#### __Rust Berry__
A special collectible that is not counted towards the total number of berries on the level's OUI card. It functionally serves the same purpose
as the Moon Berry, except it does not give the WOW achievement.

#### __Factory Activator Dash Block__
Acts like a regular dash block, with all its settings. Also counts as a dash block for all intents and purposes on your map. When broken in any way, it sends out factory activation signals. Permanent versions have their activators also permanent.

- **canDash**: Whether the player can break the block by dashing into it.
- **blendin**: Blends the block with the walls it touches, making it less apparent.
- **tiletype**: Determines the visual appearance of the wall.
- **permanent**: Once broken, the block will remain that way, even if the player dies. Also makes the factory activation permanent.
- **activationIds**: A comma separated set of string Ids for all the Activation Ids this dash block should activate when broken.

#### __Factory Activator Trigger__
This trigger lets you activate Activation Ids on entry. Can reset on exit and be permanent. Can also depend on an Id already being set.

- **activationIds**: A comma separated set of string Ids for all the Activation Ids this trigger should activate.
- **ownActivationId**: String value representing the triggers own Activation Id. When this Id is active, the trigger can fire.
- **resetOnLeave**: Boolean value that determines whether the ids activated should be reset to their previous state upon leaving. If persistent is `true`
this field is ignored. Defaults to `false`.
- **persistent**: Boolean value that determines whether the activation should be persistent even after leaving the room. If set to `true`, resetOnLeave is
ignored. Defaults to `false`.

#### __Permanent Activation Trigger__
Place this trigger on the side of a room, extending well beyond it to make it so the ids set in it get persistent after the player leaves through
that exit. Does not activate Ids on its own, but rather makes already active Ids persistent.

- **activationIds**: A comma separated set of string Ids for all the Activation Ids this trigger should activate.

#### __Factory Event Trigger__
Used to initiate cutscenes in **Shrouded Thoughts**. The trigger is removed after activation, so it can only trigger once.
Cutscenes are specific to their in-level locations so they may result in unexpected behavior when used elsewhere.

- **event**: String Id of the event to be triggered.

#### __Special Box Deactivation Trigger__
Removes the Special property from Crates that enter the trigger, either on their own or in the player's hand.

#### __Spawn Steam Wall Trigger__
Spawns a Steam Wall at the left edge of the screen that moves at 22 pixels per second by default.

- **speed**: A speed multiplier for the wall's movement speed.
- **color**: Hex color code (example: ff0000 is red) of the spawned steam wall. Default is white.

#### __Steam Wall Speed Trigger__
Changes the speed multiplier of the currently spawned steam wall. If there is no steam wall spawned currently it has no effect.

- **speed**: A speed multiplier for the wall's movement speed.

#### __Steam Wall Color Trigger__
Changes the color of the currently spawned steam wall. If there is no steam wall spawned currently it has no effect.

- **color**: Hex color code (example: ff0000 is red) of the new color. Default is white.
- **duration**: The amount of time it will take for the color change to occur in seconds.


#### __Other__

- Rusty Spike - Rusty looking spikes.
- Rusty Door - Rusty looking metal door.
- Rusty Crumble block - Rusty looknig crumble block.

### <a name=downloads>Downloads
#### Factory Helper Download
You can find the mod itself **[here](https://gamebanana.com/gamefiles/10260)**.

### <a name=dependencies>Dependencies
The mod and all maps that use it require the Everest mod loader. You can find the Everest **[here](https://gamebanana.com/tools/6449)**.

### <a name=thanks>Special thanks
I would like to thank **Cruor** for all the help in getting started with creating code mods and creating Ahorn (and Lönn) which
allowed me to create these levels. Without him this would not have been possible.

I would also like to thank **Ade** for creating and maintaining Everest. Without his work none of Celeste modding would have been possible.
We owe you a bunch!

And finally I would like to thank everyone who has given me feedback and reported bugs. You are a massive help
in developing the mod and making great and enjoyable levels!
