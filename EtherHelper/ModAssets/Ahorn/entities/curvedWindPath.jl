module EtherHelperCurvedWindPath

using ..Ahorn, Maple

@mapdef Entity "EtherHelper/CurvedWindPath" CurvedWindPath(x::Integer, y::Integer, nodes::Array{Tuple{Integer, Integer}, 1}=Tuple{Integer, Integer}[], tunnelWidth::Integer=64, strength::Real=100.0, alpha::Real=0.5)

function CurvedWindPathFinalizer(entity::CurvedWindPath)
    x, y = Int(entity.data["x"]), Int(entity.data["y"])
	entity.data["nodes"] = [(x+24,y),(x+48,y),(x+72,y)]
end

const placements = Ahorn.PlacementDict(
    "Curved Wind Path (Ether Helper)" => Ahorn.EntityPlacement(
        CurvedWindPath,
		"point",
		Dict{String, Any}(
		),
		CurvedWindPathFinalizer
    )
)

Ahorn.nodeLimits(entity::CurvedWindPath) = 3, -1

radius = 8

function Ahorn.selection(entity::CurvedWindPath)
    nodes = get(entity.data, "nodes", ())
    x, y = Ahorn.position(entity)

    res = [Ahorn.Rectangle(x - radius, y - radius, 2*radius, 2*radius)]
    
    for node in nodes
        nx, ny = Int.(node)

        push!(res, Ahorn.Rectangle(nx - radius, ny - radius, 2*radius, 2*radius))
    end

    return res
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::CurvedWindPath, room::Maple.Room)
    px, py = Ahorn.position(entity)
	Ahorn.drawCircle(ctx, px, py, 8, Ahorn.colors.selection_selected_fc)

    for node in get(entity.data, "nodes", ())
        nx, ny = Int.(node)

		Ahorn.drawCircle(ctx, nx, ny, 8, Ahorn.colors.selection_selected_fc)
        Ahorn.drawArrow(ctx, px, py, nx, ny, Ahorn.colors.selection_selected_fc, headLength=6)

        px, py = nx, ny
    end
end

end