module FactoryHelperRustyLamp

using ..Ahorn, Maple

@mapdef Entity "FactoryHelper/RustyLamp" RustyLamp(x::Integer, y::Integer, activationIds::String="", strobePattern::String="None", initialDelay::Real=0.0, startActive::Bool=false)

const placements = Ahorn.PlacementDict(
    "RustyLamp (FactoryHelper)" => Ahorn.EntityPlacement(
        RustyLamp,
        "point",
    ),
)

patterns = ["None", "LightFlicker"]

Ahorn.editingOptions(entity::RustyLamp) = Dict{String, Any}(
    "strobePattern" => patterns
)

sprite = "objects/FactoryHelper/rustyLamp/rustyLamp0"

Ahorn.nodeLimits(entity::RustyLamp) = 0, 0

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::RustyLamp, room::Maple.Room)
    x, y = Ahorn.position(entity)
    endString = String
    
    if get(entity.data, "startActive", false)
        endString = "1"
    else
        endString = "0"
    end
    
    Ahorn.drawSprite(ctx, "$(sprite)$(endString)", x + 8, y + 8)
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::RustyLamp)
end

function Ahorn.selection(entity::RustyLamp)
    x, y = Ahorn.position(entity)
    
    return Ahorn.Rectangle[Ahorn.Rectangle(x+4,y+2,8,12)]
end

end