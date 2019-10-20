module FactoryHelperSpawnSteamWallTrigger

using ..Ahorn, Maple

@mapdef Trigger "FactoryHelper/SpawnSteamWallTrigger" SpawnSteamWallTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16)

const placements = Ahorn.PlacementDict(
    "Spawn Steam Wall Trigger (FactoryHelper)" => Ahorn.EntityPlacement(
        SpawnSteamWallTrigger,
        "rectangle",
    ),
)

end