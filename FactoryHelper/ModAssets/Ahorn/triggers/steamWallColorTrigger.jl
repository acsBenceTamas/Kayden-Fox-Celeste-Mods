module FactoryHelperSteamWallColorTrigger

using ..Ahorn, Maple

@mapdef Trigger "FactoryHelper/SteamWallColorTrigger" SteamWallColorTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16, color::String="ffffff", duration::Number=1.0)

const placements = Ahorn.PlacementDict(
    "Steam Wall Color Trigger (FactoryHelper)" => Ahorn.EntityPlacement(
        SteamWallColorTrigger,
        "rectangle",
    ),
)

end
