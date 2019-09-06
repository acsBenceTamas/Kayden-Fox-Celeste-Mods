# Adventure Helper
## A Celeste mod by Kayden Fox
This mod was created for Celeste to provide additional features for custom map makers to use.
The basis for most items is decompiled code from the base game, because many of the entities in game
were not designed with the idea of extensibility. This git repo only contains the source code. To
download the mod see the [downloads](#downloads) section. The mod and levels it uses require
the Everest mod loader. You can find a link to it in the [dependencies](#dependencies) section.

**IMPORTANT!** If you ever notice any bugs with this mod or have feedback on any of the levels
linked in this document, please contact me on Discord at `Kayden Fox#4830` or in the modding
section of the official Celeste Discord server!

### Table of contents
- [Features](#features)
- [Downloads](#downloads)
- [Dependencies](#dependencies)
- [Special thanks](#thanks)

### <a name=features>Features
#### Multi-node track spinners
Comes in both Dust Sprite and Blade Spinner forms. For the most part they act like regular track spinners, 
except they can have multiple waypoints. When reaching the final node they return to their origin. Backtracking
can be achieved by placing an additional node along the path the opposite way. Instead of the preset 3 speeds
of slow, normal and fast these spinners come with customizable `moveTime` and `pauseTime` fields.
- **moveTime**: How many seconds it takes for the spinner to go from one node to the next.
- **pauseTime**: How many seconds the spinner waits at each node before moving onto the next.

#### Non-returning zip movers
These work like regular zip movers for the most part, except upon reaching their end point they do not 
immediately return to their origin, but rather wait until the player is riding them again (or still if you never
let go). They are not customizable, you can only resize and position them.

#### Linked zip movers
These come in both returning and non-returning varieties. Each linked zip mover has its own `colorCode` which
determines its zipline color and what other zip movers it is linked to. Linked zip movers send out a signal 
whenever they launch to notify all other zip movers of the same `colorCode` to launch as well. If a zip mover
is already on its way somewhere it will ignore the signal. When riding a non-returning zip mover continuously
you can make about 3 whole back and forth cycles before a regular zip mover returns to its origin.
- **colorCode**: Determines the zipline color of the zip mover as well as what other zip movers it is linked with.

### <a name=downloads>Downloads
#### Adventure Helper Download
You can find the mod itself **[here](https://gamebanana.com/gamefiles/9807)**.

#### Showcase Maps
I made some maps to showcase the more interesting features of this mod.
- **[Point of No Return](https://gamebanana.com/maps/206359)** showcases the non-returning zip movers in a short 6 screen map.
- **[Synchronized Jumping](https://gamebanana.com/maps/206380)** showcases the linked zip movers in a short 8 screen map.

### <a name=dependencies>Dependencies
The mod and all maps that use it require the Everest mod loader. You can find the Everest **[here](https://gamebanana.com/tools/6449)**.

### <a name=thanks>Special thanks
I would like to thank **Cruor** for all the help in getting started with creating code mods and creating Ahorn (and Lönn) which
allowed me to create these levels. Without him this would not have been possible.

I would also like to thank **Ade** for creating and maintaining Everest. Without his work none of Celeste modding would have been possible.
We owe you a bunch!

And finally I would like to thank everyone who has given me feedback and reported bugs. You are a massive help
in developing the mod and making great and enjoyable levels!