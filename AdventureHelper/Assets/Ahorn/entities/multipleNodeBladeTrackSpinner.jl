module AdventureHelperBladeTrackSpinnerMultinode

using ..Ahorn, Maple

@mapdef Entity "AdventureHelper/BladeTrackSpinnerMultinode" BladeTrackSpinnerMultinode(x::Integer, y::Integer, nodes::Array{Tuple{Integer, Integer}, 1}=Tuple{Integer, Integer}[], moveTime::Real=0.4, pauseTime::Real=0.2)

function MultinodeBladeSpinnerFinalizer(entity::BladeTrackSpinnerMultinode)
    x, y = Int(entity.data["x"]), Int(entity.data["y"])
	entity.data["nodes"] = [(x+24,y)]
end

const placements = Ahorn.PlacementDict(
    "Blade (Track Multi-Node) (Adventure Helper)" => Ahorn.EntityPlacement(
        BladeTrackSpinnerMultinode,
		"point",
		Dict{String, Any}(
		),
		MultinodeBladeSpinnerFinalizer
    )
)

Ahorn.nodeLimits(entity::BladeTrackSpinnerMultinode) = 1, -1

sprite = "danger/blade00"

function Ahorn.selection(entity::BladeTrackSpinnerMultinode)
    nodes = get(entity.data, "nodes", ())
    x, y = Ahorn.position(entity)

    res = Ahorn.Rectangle[Ahorn.getSpriteRectangle(sprite, x, y)]
    
    for node in nodes
        nx, ny = Int.(node)

        push!(res, Ahorn.getSpriteRectangle(sprite, nx, ny))
    end

    return res
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::BladeTrackSpinnerMultinode)
    px, py = Ahorn.position(entity)

    for node in get(entity.data, "nodes", ())
        nx, ny = Int.(node)

        Ahorn.drawSprite(ctx, sprite, nx, ny)
        Ahorn.drawArrow(ctx, px, py, nx, ny, Ahorn.colors.selection_selected_fc, headLength=6)

        px, py = nx, ny
    end
	nx, ny = Ahorn.position(entity)
	
    Ahorn.drawArrow(ctx, px, py, nx, ny, Ahorn.colors.selection_selected_fc, headLength=6)
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::BladeTrackSpinnerMultinode, room::Maple.Room)
    x, y = Ahorn.position(entity)
    Ahorn.drawSprite(ctx, sprite, x, y)
end

end