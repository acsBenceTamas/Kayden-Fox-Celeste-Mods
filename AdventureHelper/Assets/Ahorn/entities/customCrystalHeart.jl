module AdventureHelperCustomCrystalHeart

using ..Ahorn, Maple

@mapdef Entity "AdventureHelper/CustomCrystalHeart" CustomCrystalHeart(x::Integer, y::Integer, color::String="00a81f", path::String="")

const placements = Ahorn.PlacementDict(
    "Custom Crystal Heart (Custom, Adventure Helper)" => Ahorn.EntityPlacement(
        CustomCrystalHeart,
		"point",
        Dict{String, Any}( "path" => ""),
    ),
    "Custom Crystal Heart (Blue, Adventure Helper)" => Ahorn.EntityPlacement(
        CustomCrystalHeart,
		"point",
        Dict{String, Any}( "path" => "heartgem0"),
    ),
    "Custom Crystal Heart (Red, Adventure Helper)" => Ahorn.EntityPlacement(
        CustomCrystalHeart,
		"point",
        Dict{String, Any}( "path" => "heartgem1"),
    ),
    "Custom Crystal Heart (Gold, Adventure Helper)" => Ahorn.EntityPlacement(
        CustomCrystalHeart,
		"point",
        Dict{String, Any}( "path" => "heartgem2"),
    ),
    "Custom Crystal Heart (White, Adventure Helper)" => Ahorn.EntityPlacement(
        CustomCrystalHeart,
		"point",
        Dict{String, Any}( "path" => "heartgem3"),
    ),
)

function Ahorn.selection(entity::CustomCrystalHeart)
    x, y = Ahorn.position(entity)

	sprite = "collectables/heartGem/3/00.png"
    return Ahorn.getSpriteRectangle(sprite, x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CustomCrystalHeart, room::Maple.Room)
	path = get(entity.data, "path", "")
	sprite = String
	
	if path == "heartgem0"
		sprite = "collectables/heartGem/0/00.png"
	elseif path == "heartgem1"
		sprite = "collectables/heartGem/1/00.png"
	elseif path == "heartgem2"
		sprite = "collectables/heartGem/2/00.png"
	elseif path == "heartgem3"
		sprite = "collectables/heartGem/3/00.png"
	elseif path == ""
		sprite = "collectables/AdventureHelper/RecolorHeart_Outline/00.png"
		tint = Ahorn.argb32ToRGBATuple(parse(Int, get(entity.data, "color", "663931"), base=16))[1:3] ./ 255
		tint = (tint[1], tint[2], tint[3], 1.0)
		Ahorn.drawSprite(ctx, "collectables/AdventureHelper/RecolorHeart/00.png", 0, 0, tint=tint)
	else
		sprite = "collectables/heartGem/3/00.png"
	end
	
	Ahorn.drawSprite(ctx, sprite, 0, 0)
end

end