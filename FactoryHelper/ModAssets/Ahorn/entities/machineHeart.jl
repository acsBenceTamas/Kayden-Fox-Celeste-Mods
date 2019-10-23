module FactoryHelperMachineHeart

using ..Ahorn, Maple

@mapdef Entity "FactoryHelper/MachineHeart" MachineHeart(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Machine Heart (FactoryHelper)" => Ahorn.EntityPlacement(
        MachineHeart,
        "point",
    ),
)

sprite = "objects/FactoryHelper/machineHeart/front0"

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::MachineHeart, room::Maple.Room)
    x, y = Ahorn.position(entity)
    Ahorn.drawSprite(ctx, sprite, x, y)
end

function Ahorn.selection(entity::MachineHeart)
    x, y = Ahorn.position(entity)
    
    return Ahorn.Rectangle[Ahorn.Rectangle(x-12,y-16,24,32)]
end

end