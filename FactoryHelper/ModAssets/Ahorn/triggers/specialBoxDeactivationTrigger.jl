module FactoryHelperSpecialBoxDeactivationTrigger

using ..Ahorn, Maple

@mapdef Trigger "FactoryHelper/SpecialBoxDeactivationTrigger" SpecialBoxDeactivationTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16)

const placements = Ahorn.PlacementDict(
    "Special Box Deactivation Trigger (FactoryHelper)" => Ahorn.EntityPlacement(
        SpecialBoxDeactivationTrigger,
        "rectangle",
    ),
)

end