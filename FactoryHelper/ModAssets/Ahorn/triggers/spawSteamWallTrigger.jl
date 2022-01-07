module FactoryHelperSpawnSteamWallTrigger

using ..Ahorn, Maple

@mapdef Trigger "FactoryHelper/SpawnSteamWallTrigger" SpawnSteamWallTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16, speed::Number=1.0, color::String="ffffff")

const placements = Ahorn.PlacementDict(
    "Spawn Steam Wall Trigger (FactoryHelper)" => Ahorn.EntityPlacement(
        SpawnSteamWallTrigger,
        "rectangle",
    ),
)

end
