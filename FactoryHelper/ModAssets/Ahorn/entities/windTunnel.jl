module FactoryHelperWindTunnel

using ..Ahorn, Maple

@mapdef Entity "FactoryHelper/WindTunnel" WindTunnel(x::Integer, y::Integer, width::Integer=16, height::Integer=16, direction::String="Up", activationId::String="", strength::Real=1.0, startActive::Bool=false)

directions = ["Up", "Down", "Left", "Right"]

const placements = Ahorn.PlacementDict(
    "WindTunnel ($(direction)) (FactoryHelper)" => Ahorn.EntityPlacement(
        WindTunnel,
        "rectangle",
        Dict{String, Any}(
            "direction" => direction,
        )
    ) for direction in directions
)

function Ahorn.minimumSize(entity::WindTunnel)
    return (16, 16)
end

Ahorn.nodeLimits(entity::WindTunnel) = 0, 0

Ahorn.editingOptions(entity::WindTunnel) = Dict{String, Any}(
    "direction" => directions
)
Ahorn.resizable(entity::WindTunnel) = true, true

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::WindTunnel, room::Maple.Room)
    x, y = Ahorn.position(entity)
    width = get(entity.data, "width", 16)
    height = get(entity.data, "height", 16)
    
    Ahorn.drawRectangle(ctx, x, y, width, height, (0.7, 0.7, 0.7, 0.4), (0.7, 0.7, 0.7, 1.0))
    
    xf = xt = x
    yf = yt = y
    
    direction = get(entity.data, "direction", "Up")
    
    if direction == "Up" || direction == "Down"
        xf = xt = x + width/2
    elseif direction == "Left" || direction == "Right"
        yf = yt = y + height/2
    end
    if direction == "Up"
        yf = y + height
        yt = y
    elseif direction == "Down"
        yf = y
        yt = y + height
    elseif direction == "Left"
        xf = x + width
        xt = x
    elseif direction == "Right"
        xf = x
        xt = x + width
    end
    
    Ahorn.drawArrow(ctx, xf, yf, xt, yt, (0.0, 0.0, 0.7, 1.0), headLength=4)
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::WindTunnel)
end

function Ahorn.selection(entity::WindTunnel)
    x, y = Ahorn.position(entity)
    width = get(entity.data, "width", 16)
    height = get(entity.data, "height", 16)
    
    return Ahorn.Rectangle[Ahorn.Rectangle(x,y,width,height)]
end

end