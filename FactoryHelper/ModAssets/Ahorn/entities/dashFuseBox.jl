module FactoryHelperDashFuseBox

using ..Ahorn, Maple

@mapdef Entity "FactoryHelper/DashFuseBox" DashFuseBox(x::Integer, y::Integer, activationIds::String="", persistent::Bool=false)

const placements = Ahorn.PlacementDict(
    "DashFuseBox (Right) (FactoryHelper)" => Ahorn.EntityPlacement(
        DashFuseBox,
        "point",
        Dict{String, Any}(
            "direction" => "Right",
        )
    ),
    "DashFuseBox (Left) (FactoryHelper)" => Ahorn.EntityPlacement(
        DashFuseBox,
        "point",
        Dict{String, Any}(
            "direction" => "Left",
        )
    ),
)

directions = String["Left", "Right"]

Ahorn.editingOptions(entity::DashFuseBox) = Dict{String, Any}(
    "direction" => directions
)

Ahorn.nodeLimits(entity::DashFuseBox) = 0, 0

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::DashFuseBox, room::Maple.Room)
    x, y = Ahorn.position(entity)
    
    if entity.data["direction"] == "Right"
        Ahorn.drawRectangle(ctx, x, y, 4, 16, (0.5, 0.4, 0.3, 0.4), (0.5, 0.4, 0.3, 1.0))
        Ahorn.drawRectangle(ctx, x + 4, y, 12, 16, (0.5, 0.4, 0.3, 0.2), (0.5, 0.4, 0.3, 0.5))
    else
        Ahorn.drawRectangle(ctx, x -4, y, 4, 16, (0.5, 0.4, 0.3, 0.4), (0.5, 0.4, 0.3, 1.0))
        Ahorn.drawRectangle(ctx, x -16, y, 12, 16, (0.5, 0.4, 0.3, 0.2), (0.5, 0.4, 0.3, 0.5))
    end
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::DashFuseBox)
    x, y = Ahorn.position(entity)
        
    if entity.data["direction"] == "Right"
        Ahorn.drawArrow(ctx, x, y+8, x+16, y+8, Ahorn.colors.selection_selected_fc, headLength=4)
    else
        Ahorn.drawArrow(ctx, x, y+8, x-16, y+8, Ahorn.colors.selection_selected_fc, headLength=4)
    end
end

function Ahorn.selection(entity::DashFuseBox)
    x, y = Ahorn.position(entity)
    
    if entity.data["direction"] == "Right"
        res = Ahorn.Rectangle[Ahorn.Rectangle(x,y,4,16)]
    else
        res = Ahorn.Rectangle[Ahorn.Rectangle(x-4,y,4,16)]
    end
    
    return res
end

end