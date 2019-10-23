module FactoryHelperEventTrigger

using ..Ahorn, Maple

@mapdef Trigger "FactoryHelper/EventTrigger" EventTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16, event::String="")

const placements = Ahorn.PlacementDict(
    "Factory Event Trigger (FactoryHelper)" => Ahorn.EntityPlacement(
        EventTrigger,
        "rectangle",
    ),
)

end