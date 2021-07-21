module AdventureHelperDusteTrackSpinnerMultinode

using ..Ahorn, Maple

@mapdef Entity "AdventureHelper/DustTrackSpinnerMultinode" DusteTrackSpinnerMultinode(x::Integer, y::Integer, nodes::Array{Tuple{Integer, Integer}, 1}=Tuple{Integer, Integer}[], moveTime::Real=0.4, pauseTime::Real=0.2)

function MultinodeDustSpinnerFinalizer(entity::DusteTrackSpinnerMultinode)
    x, y = Int(entity.data["x"]), Int(entity.data["y"])
	entity.data["nodes"] = [(x+24,y)]
end

const placements = Ahorn.PlacementDict(
    "Dust Sprite (Track Multi-Node) (Adventure Helper)" => Ahorn.EntityPlacement(
        DusteTrackSpinnerMultinode,
		"point",
		Dict{String, Any}(
		),
		MultinodeDustSpinnerFinalizer
    )
)

Ahorn.nodeLimits(entity::DusteTrackSpinnerMultinode) = 1, -1

sprite1 = "danger/dustcreature/base00"
sprite2 = "danger/dustcreature/center00"

function Ahorn.selection(entity::DusteTrackSpinnerMultinode)
    nodes = get(entity.data, "nodes", ())
    x, y = Ahorn.position(entity)

    res = Ahorn.Rectangle[Ahorn.getSpriteRectangle(sprite1, x, y)]
    
    for node in nodes
        nx, ny = Int.(node)

        push!(res, Ahorn.getSpriteRectangle(sprite1, nx, ny))
    end

    return res
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::DusteTrackSpinnerMultinode)
    px, py = Ahorn.position(entity)

    for node in get(entity.data, "nodes", ())
        nx, ny = Int.(node)

        Ahorn.drawSprite(ctx, sprite1, nx, ny)
		Ahorn.drawSprite(ctx, sprite2, nx, ny)
        Ahorn.drawArrow(ctx, px, py, nx, ny, Ahorn.colors.selection_selected_fc, headLength=6)

        px, py = nx, ny
    end
	nx, ny = Ahorn.position(entity)
	
    Ahorn.drawArrow(ctx, px, py, nx, ny, Ahorn.colors.selection_selected_fc, headLength=6)
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::DusteTrackSpinnerMultinode, room::Maple.Room)
    x, y = Ahorn.position(entity)
	Ahorn.drawSprite(ctx, sprite1, x, y)
	Ahorn.drawSprite(ctx, sprite2, x, y)
end

end