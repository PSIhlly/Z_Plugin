$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$taskRecord="D:/Works/Unity's/Personal/Z_Plugin/Z_Plugin/ExtraAssets/使用中/贴图/wang_sand_feather_像素化记录_20261023"
Copy-Item -LiteralPath "C:\Users\LH1\.codex\generated_images\01a11a07-4b7c-7991-a4d8-b119a9a411d7\exec-7c31d1a5-3c8d-4c23-ac23-4a0d1a79d486.png" -Destination (Join-Path $taskRecord '生成原图.png')
$taskGen=[System.Drawing.Bitmap]::new((Join-Path $taskRecord '生成原图.png'))
$taskOriginal=[System.Drawing.Bitmap]::new((Join-Path $taskRecord '原图.png'))
$taskReference=[System.Drawing.Bitmap]::new((Join-Path $taskRecord '颜色参考.png'))
$taskBase=$taskReference.GetPixel(128,128)
$taskValues=[double[]]::new(96*144)
$taskAlphas=[int[]]::new(192*288)
$taskSum=0.0
for($taskY=0;$taskY -lt 144;$taskY++){for($taskX=0;$taskX -lt 96;$taskX++){
 $taskLum=0.0
 for($taskSY=0;$taskSY -lt 2;$taskSY++){for($taskSX=0;$taskSX -lt 2;$taskSX++){
  $taskGX=[Math]::Min($taskGen.Width-1,[int][Math]::Floor(($taskX+($taskSX+0.5)/2)*$taskGen.Width/96))
  $taskGY=[Math]::Min($taskGen.Height-1,[int][Math]::Floor(($taskY+($taskSY+0.5)/2)*$taskGen.Height/144))
  $taskC=$taskGen.GetPixel($taskGX,$taskGY)
  $taskA=$taskC.A/255.0
  $taskR=$taskC.R*$taskA+$taskBase.R*(1-$taskA)
  $taskG=$taskC.G*$taskA+$taskBase.G*(1-$taskA)
  $taskB=$taskC.B*$taskA+$taskBase.B*(1-$taskA)
  $taskLum += 0.2126*$taskR+0.7152*$taskG+0.0722*$taskB
 }}
 $taskValue=$taskLum/4
 $taskValues[$taskY*96+$taskX]=$taskValue
 $taskSum += $taskValue
}}
$taskAverage=$taskSum/$taskValues.Length
$taskDst=[System.Drawing.Bitmap]::new(192,288,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$taskOpaque=0;$taskTransparent=0
$taskHistogram=@{}
for($taskY=0;$taskY -lt 288;$taskY++){for($taskX=0;$taskX -lt 192;$taskX++){
 $taskAlpha=$taskOriginal.GetPixel([Math]::Min($taskOriginal.Width-1,2*$taskX+1),[Math]::Min($taskOriginal.Height-1,2*$taskY+1)).A
 if($taskAlpha -eq 0){$taskTransparent++;continue}
 $taskDelta=$taskValues[([int][Math]::Floor($taskY/2))*96+([int][Math]::Floor($taskX/2))]-$taskAverage
 $taskLevel=[int][Math]::Round($taskDelta/4.0)
 $taskLevel=[Math]::Max(-3,[Math]::Min(3,$taskLevel))
 $taskOffset=4*$taskLevel
 $taskColor=[System.Drawing.Color]::FromArgb($taskAlpha,$taskBase.R+$taskOffset,$taskBase.G+$taskOffset,$taskBase.B+$taskOffset)
 $taskDst.SetPixel($taskX,$taskY,$taskColor)
 $taskOpaque++
 $taskKey="$($taskColor.R),$($taskColor.G),$($taskColor.B)"
 $taskHistogram[$taskKey]=1+$taskHistogram[$taskKey]
}}
$taskGen.Dispose();$taskOriginal.Dispose();$taskReference.Dispose()
$taskDst.Save((Join-Path $taskRecord 'wang_sand_feather_192x288.png'),[System.Drawing.Imaging.ImageFormat]::Png)
$taskPreview=[System.Drawing.Bitmap]::new(576,864)
$taskGraphics=[System.Drawing.Graphics]::FromImage($taskPreview)
$taskGraphics.Clear([System.Drawing.Color]::FromArgb(70,72,76))
$taskGraphics.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
$taskGraphics.PixelOffsetMode=[System.Drawing.Drawing2D.PixelOffsetMode]::Half
$taskGraphics.DrawImage($taskDst,[System.Drawing.Rectangle]::new(0,0,576,864))
$taskGraphics.Dispose()
$taskPreview.Save((Join-Path $taskRecord '预览_3倍.png'),[System.Drawing.Imaging.ImageFormat]::Png)
$taskPreview.Dispose();$taskDst.Dispose()
[pscustomobject]@{Width=192;Height=288;Opaque=$taskOpaque;Transparent=$taskTransparent;BaseRGB="$($taskBase.R),$($taskBase.G),$($taskBase.B)";Palette=$taskHistogram} | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $taskRecord '尺寸色板检查.json') -Encoding utf8
Get-Content -LiteralPath (Join-Path $taskRecord '尺寸色板检查.json') -Raw

