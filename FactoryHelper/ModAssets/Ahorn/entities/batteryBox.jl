module FactoryHelperBatteryBox

using ..Ahorn, Maple

@mapdef Entity "FactoryHelper/BatteryBox" BatteryBox(x::Integer, y::Integer, activationIds::String="")
@mapdef Entity "FactoryHelper/Battery" Battery(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Battery Box (FactoryHelper)" => Ahorn.EntityPlacement(
        BatteryBox,
        "point",
    ),
    "Battery (FactoryHelper)" => Ahorn.EntityPlacement(
        Battery,
        "point",
    ),
)

spriteBox = "objects/FactoryHelper/batteryBox/inactive0"
spriteBattery = "objects/FactoryHelper/batteryBox/battery00"

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::BatteryBox, room::Maple.Room)
    x, y = Ahorn.position(entity)
    
    Ahorn.drawSprite(ctx, spriteBox, x, y)
end

function Ahorn.selection(entity::BatteryBox)
    x, y = Ahorn.position(entity)
    
    return Ahorn.Rectangle[Ahorn.Rectangle(x-16,y-16,32,32)]
end


function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::Battery, room::Maple.Room)
    x, y = Ahorn.position(entity)
    
    Ahorn.drawSprite(ctx, spriteBattery, x, y)
end

function Ahorn.selection(entity::Battery)
    x, y = Ahorn.position(entity)
    
    return Ahorn.Rectangle[Ahorn.Rectangle(x-6,y-8,12,16)]
end

end