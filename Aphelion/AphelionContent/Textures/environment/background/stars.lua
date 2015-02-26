setCanvasSize(1024, 1024)
enableAntiAliasing(false)

--drawRect(0, 0, 1024, 1024, makeColor(0, 0, 0))
print("The black background is just for testing purposes.")
print("Comment out that line before saving it.")

for i = 1, 128 do
local starcolor = makeColorAlpha(math.random(255), 255, 255, 255)
drawPixel(math.random(1024), math.random(1024), starcolor)
end