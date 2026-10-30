$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$taskRoot="D:/Works/Unity's/Personal/Z_Plugin/Z_Plugin/ExtraAssets/使用中/贴图"
$taskRecord=Join-Path $taskRoot 'wang_rock_feather_像素化记录_20261030'
Copy-Item -LiteralPath "C:\Users\LH1\.codex\generated_images\01a11a07-4b7c-7991-a4d8-b119a9a411d7\exec-69ce902f-56e4-40a7-9fcc-7144fa87011f.png" -Destination (Join-Path $taskRecord '生成原图.png')
$taskGen=[System.Drawing.Bitmap]::new((Join-Path $taskRecord '生成原图.png'))
$taskOriginal=[System.Drawing.Bitmap]::new((Join-Path $taskRecord '原图.png'))
$taskSR=0.0;$taskSG=0.0;$taskSB=0.0;$taskCount=0
for($taskY=0;$taskY -lt 288;$taskY++){for($taskX=0;$taskX -lt 192;$taskX++){
 $taskC=$taskOriginal.GetPixel(2*$taskX+1,2*$taskY+1)
 if($taskC.A -eq 255){$taskSR+=$taskC.R;$taskSG+=$taskC.G;$taskSB+=$taskC.B;$taskCount++}
}}
if($taskCount -lt 100){throw 'No opaque reference samples'}
$taskBaseR=[int][Math]::Round($taskSR/$taskCount)
$taskBaseG=[int][Math]::Round($taskSG/$taskCount)
$taskBaseB=[int][Math]::Round($taskSB/$taskCount)
$taskBaseLum=0.2126*$taskBaseR+0.7152*$taskBaseG+0.0722*$taskBaseB
$taskValues=[double[]]::new(192*288)
$taskAlphas=[int[]]::new(192*288)
$taskLumSum=0.0
for($taskY=0;$taskY -lt 288;$taskY++){for($taskX=0;$taskX -lt 192;$taskX++){
 $taskGX=[Math]::Min($taskGen.Width-1,[int][Math]::Floor(($taskX+0.5)*$taskGen.Width/192))
 $taskGY=[Math]::Min($taskGen.Height-1,[int][Math]::Floor(($taskY+0.5)*$taskGen.Height/288))
 $taskC=$taskGen.GetPixel($taskGX,$taskGY)
 $taskA=$taskC.A/255.0
 $taskLum=(0.2126*$taskC.R+0.7152*$taskC.G+0.0722*$taskC.B)*$taskA+$taskBaseLum*(1-$taskA)
 $taskK=$taskY*192+$taskX
 $taskValues[$taskK]=$taskLum
 $taskAlphas[$taskK]=$taskOriginal.GetPixel(2*$taskX+1,2*$taskY+1).A
 if($taskAlphas[$taskK] -eq 255){$taskLumSum+=$taskLum}
}}
$taskLumMean=$taskLumSum/$taskCount
$taskDst=[System.Drawing.Bitmap]::new(192,288,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$taskHistogram=@{}
$taskOpaque=0;$taskTransparent=0;$taskPartial=0
for($taskY=0;$taskY -lt 288;$taskY++){for($taskX=0;$taskX -lt 192;$taskX++){
 $taskK=$taskY*192+$taskX
 $taskA=$taskAlphas[$taskK]
 if($taskA -eq 0){$taskTransparent++;continue}
 if($taskA -eq 255){$taskOpaque++}else{$taskPartial++}
 $taskDelta=($taskValues[$taskK]-$taskLumMean)*0.9
 $taskLevel=[int][Math]::Round($taskDelta/8.0)
 $taskLevel=[Math]::Max(-4,[Math]::Min(4,$taskLevel))
 $taskOffset=8*$taskLevel
 $taskColor=[System.Drawing.Color]::FromArgb($taskA,$taskBaseR+$taskOffset,$taskBaseG+$taskOffset,$taskBaseB+$taskOffset)
 $taskDst.SetPixel($taskX,$taskY,$taskColor)
 $taskKey="$($taskColor.R),$($taskColor.G),$($taskColor.B)"
 $taskHistogram[$taskKey]=1+$taskHistogram[$taskKey]
}}
$taskGen.Dispose();$taskOriginal.Dispose()
$taskFinal=Join-Path $taskRecord 'wang_rock_feather_192x288.png'
$taskDst.Save($taskFinal,[System.Drawing.Imaging.ImageFormat]::Png)
$taskPreview=[System.Drawing.Bitmap]::new(576,864)
$taskG=[System.Drawing.Graphics]::FromImage($taskPreview)
$taskG.Clear([System.Drawing.Color]::FromArgb(65,67,71))
$taskG.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
$taskG.PixelOffsetMode=[System.Drawing.Drawing2D.PixelOffsetMode]::Half
$taskG.DrawImage($taskDst,[System.Drawing.Rectangle]::new(0,0,576,864))
$taskG.Dispose()
$taskPreview.Save((Join-Path $taskRecord '预览_3倍.png'),[System.Drawing.Imaging.ImageFormat]::Png)
$taskPreview.Dispose();$taskDst.Dispose()
[pscustomobject]@{Width=192;Height=288;Opaque=$taskOpaque;Transparent=$taskTransparent;PartialAlpha=$taskPartial;BaseRGB="$taskBaseR,$taskBaseG,$taskBaseB";Palette=$taskHistogram} | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $taskRecord '尺寸色板检查.json') -Encoding utf8
Get-Content -LiteralPath (Join-Path $taskRecord '尺寸色板检查.json') -Raw

