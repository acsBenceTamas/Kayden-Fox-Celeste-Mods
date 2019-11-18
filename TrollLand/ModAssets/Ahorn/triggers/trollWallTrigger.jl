module TrollLandTrollWallTrigger

using ..Ahorn, Maple

@mapdef Trigger "TrollLand/TrollWallTrigger" TrollWallTrigger(x::Integer, y::Integer, width::Integer=8, height::Integer=8; idString::String="")

const placements = Ahorn.PlacementDict(
    "Troll Wall (Troll Land)" => Ahorn.EntityPlacement(
        TrollWallTrigger,
        "rectangle"
    )
)

end