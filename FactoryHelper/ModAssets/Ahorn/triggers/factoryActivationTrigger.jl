module FactoryHelperFactoryActivationTrigger

using ..Ahorn, Maple

@mapdef Trigger "FactoryHelper/FactoryActivationTrigger" FactoryActivationTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16, activationIds::String="", ownActivationId::String="", resetOnLeave::Bool=false, persistent::Bool=false)

const placements = Ahorn.PlacementDict(
    "Factory Activation Trigger (FactoryHelper)" => Ahorn.EntityPlacement(
        FactoryActivationTrigger,
        "rectangle",
    ),
)

end