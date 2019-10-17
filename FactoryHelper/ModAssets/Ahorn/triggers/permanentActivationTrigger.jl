module FactoryHelperPermanentActivationTrigger

using ..Ahorn, Maple

@mapdef Trigger "FactoryHelper/PermanentActivationTrigger" PermanentActivationTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16, activationIds::String="")

const placements = Ahorn.PlacementDict(
    "Permanent Activation Trigger (FactoryHelper)" => Ahorn.EntityPlacement(
        PermanentActivationTrigger,
        "rectangle",
    ),
)

end