module AdventureHelperLinkedZipMover

using ..Ahorn, Maple

@pardef LinkedZipMover(x1::Integer, y1::Integer, x2::Integer=x1 + 16, y2::Integer=y1, width::Integer=16, height::Integer=16, colorCode::String="ff0000", spritePath::String="") = Entity("AdventureHelper/LinkedZipMover", x=x1, y=y1, nodes=Tuple{Int, Int}[(x2, y2)], width=width, height=height , colorCode=colorCode, spritePath=spritePath)

const placements = Ahorn.PlacementDict(
    "Zip Mover (Synced) (Adventure Helper)" => Ahorn.EntityPlacement(
        LinkedZipMover,
        "rectangle",
        Dict{String, Any}(),
        function(entity)
            entity.data["nodes"] = [(Int(entity.data["x"]) + Int(entity.data["width"]) + 8, Int(entity.data["y"]))]
        end
    ),
    "Zip Mover (Synced, Custom Speed) (Adventure Helper)" => Ahorn.EntityPlacement(
        LinkedZipMover,
        "rectangle",
        Dict{String, Any}( "speedMultiplier" => 1.0),
        function(entity)
            entity.data["nodes"] = [(Int(entity.data["x"]) + Int(entity.data["width"]) + 8, Int(entity.data["y"]))]
        end
    ),
)

Ahorn.nodeLimits(entity::LinkedZipMover) = 1, 1

Ahorn.minimumSize(entity::LinkedZipMover) = 16, 16
Ahorn.resizable(entity::LinkedZipMover) = true, true

function Ahorn.selection(entity::LinkedZipMover)
    x, y = Ahorn.position(entity)
    nx, ny = Int.(entity.data["nodes"][1])

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    return [Ahorn.Rectangle(x, y, width, height), Ahorn.Rectangle(nx + floor(Int, width / 2) - 5, ny + floor(Int, height / 2) - 5, 10, 10)]
end

ropeColor = (255, 0, 0) ./ 255
frame = "objects/zipmover/block"
light = "objects/zipmover/light01"

function renderZipMover(ctx::Ahorn.Cairo.CairoContext, x::Number, y::Number, width::Number, height::Number, nx::Number, ny::Number, colorCode::String)
    lightSprite = Ahorn.getSprite(light, "Gameplay")
	
	ropeColor = Ahorn.argb32ToRGBATuple(parse(Int, colorCode, base=16))[1:3] ./ 255
	realRopeColor = (ropeColor..., 1.0)

    tilesWidth = div(width, 8)
    tilesHeight = div(height, 8)

    cx, cy = x + width / 2, y + height / 2
    cnx, cny = nx + width / 2, ny + height / 2

    length = sqrt((x - nx)^2 + (y - ny)^2)
    theta = atan(cny - cy, cnx - cx)

    Ahorn.Cairo.save(ctx)

    Ahorn.translate(ctx, cx, cy)
    Ahorn.rotate(ctx, theta)

    Ahorn.setSourceColor(ctx, realRopeColor)
    Ahorn.set_antialias(ctx, 1)
    Ahorn.set_line_width(ctx, 1);

    # Offset for rounding errors
    Ahorn.move_to(ctx, 0, 4 + (theta <= 0))
    Ahorn.line_to(ctx, length, 4 + (theta <= 0))

    Ahorn.move_to(ctx, 0, -4 - (theta > 0))
    Ahorn.line_to(ctx, length, -4 - (theta > 0))

    Ahorn.stroke(ctx)

    Ahorn.Cairo.restore(ctx)

    Ahorn.drawRectangle(ctx, x + 2, y + 2, width - 4, height - 4, (0.0, 0.0, 0.0, 1.0))
    Ahorn.drawSprite(ctx, "objects/zipmover/cog.png", cnx, cny)

    for i in 2:tilesWidth - 1
        Ahorn.drawImage(ctx, frame, x + (i - 1) * 8, y, 8, 0, 8, 8)
        Ahorn.drawImage(ctx, frame, x + (i - 1) * 8, y + height - 8, 8, 16, 8, 8)
    end

    for i in 2:tilesHeight - 1
        Ahorn.drawImage(ctx, frame, x, y + (i - 1) * 8, 0, 8, 8, 8)
        Ahorn.drawImage(ctx, frame, x + width - 8, y + (i - 1) * 8, 16, 8, 8, 8)
    end

    Ahorn.drawImage(ctx, frame, x, y, 0, 0, 8, 8)
    Ahorn.drawImage(ctx, frame, x + width - 8, y, 16, 0, 8, 8)
    Ahorn.drawImage(ctx, frame, x, y + height - 8, 0, 16, 8, 8)
    Ahorn.drawImage(ctx, frame, x + width - 8, y + height - 8, 16, 16, 8, 8)

    Ahorn.drawImage(ctx, lightSprite, x + floor(Int, (width - lightSprite.width) / 2), y)
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::LinkedZipMover, room::Maple.Room)
    x, y = Ahorn.position(entity)
    nx, ny = Int.(entity.data["nodes"][1])

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))
	
	colorCode = get(entity.data, "colorCode", "ffffff")

    renderZipMover(ctx, x, y, width, height, nx, ny, colorCode)
end

end