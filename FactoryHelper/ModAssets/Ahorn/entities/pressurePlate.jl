module FactoryHelperPressurePlate

using ..Ahorn, Maple

@mapdef Entity "FactoryHelper/PressurePlate" PressurePlate(x::Integer, y::Integer, activationIds::String="")

const placements = Ahorn.PlacementDict(
    "Pressure Plate (FactoryHelper)" => Ahorn.EntityPlacement(
        PressurePlate,
        "point",
    ),
)

buttonSprite = "objects/FactoryHelper/pressurePlate/plate_button0"
caseSprite = "objects/FactoryHelper/pressurePlate/plate_case0"

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::PressurePlate, room::Maple.Room)
    x, y = Ahorn.position(entity)
    
    Ahorn.drawSprite(ctx, buttonSprite, x + 8, y + 8)
    Ahorn.drawSprite(ctx, caseSprite, x + 8, y + 8)
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::PressurePlate)
end

function Ahorn.selection(entity::PressurePlate)
    x, y = Ahorn.position(entity)
    
    return Ahorn.Rectangle[Ahorn.Rectangle(x,y,16,16)]
end

end