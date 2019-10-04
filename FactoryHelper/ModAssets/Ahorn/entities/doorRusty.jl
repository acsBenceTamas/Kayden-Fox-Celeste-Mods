module FactoryHelperDoorRusty

using ..Ahorn, Maple

@mapdef Entity "FactoryHelper/DoorRusty" DoorRusty(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Door (Rusty) (FactoryHelper)" => Ahorn.EntityPlacement(
        DoorRusty,
    )
)

sprite = "objects/FactoryHelper/doorRusty/metaldoor00"

function Ahorn.selection(entity::DoorRusty)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x-2, y-24, 4, 24)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DoorRusty, room::Maple.Room)
    Ahorn.drawSprite(ctx, sprite, 0, 0, jx=0.5, jy=1.0)
end

end