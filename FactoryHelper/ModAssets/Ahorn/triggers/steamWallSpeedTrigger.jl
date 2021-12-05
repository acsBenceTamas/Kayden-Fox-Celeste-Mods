module FactoryHelperSteamWallSpeedTrigger

using ..Ahorn, Maple

@mapdef Trigger "FactoryHelper/SteamWallSpeedTrigger" SteamWallSpeedTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16, speed::Number=1.0)

const placements = Ahorn.PlacementDict(
    "Steam Wall Speed Trigger (FactoryHelper)" => Ahorn.EntityPlacement(
        SteamWallSpeedTrigger,
        "rectangle",
    ),
)

end
