module TrollLandSoftlockTrigger

using ..Ahorn, Maple

@mapdef Trigger "TrollLand/SoftlockTrigger" SoftlockTrigger(x::Integer, y::Integer, width::Integer=8, height::Integer=8)

const placements = Ahorn.PlacementDict(
    "Soft Lock (Troll Land)" => Ahorn.EntityPlacement(
        SoftlockTrigger,
        "rectangle"
    )
)

end