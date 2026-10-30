param([string]$Record)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$taskDir=Split-Path ([System.Drawing.Bitmap].Assembly.Location)
$taskRefs=@([System.Drawing.Bitmap].Assembly.Location,[System.Drawing.Rectangle].Assembly.Location,(Join-Path $PSHOME 'ref/System.Runtime.dll'),(Join-Path $taskDir 'System.Private.Windows.GdiPlus.dll'),(Join-Path $taskDir 'System.Private.Windows.Core.dll'))
$taskCode=@'
using System;using System.Drawing;
public static class CabinetAsset {
 public static void Prepare(string source,string master,string dest,int width,int height){
 using(Bitmap s=new Bitmap(source)){
 int w=s.Width,h=s.Height,n=w*h;int[] pixels=new int[n],queue=new int[n];byte[] dist=new byte[n];bool[] mask=new bool[n];int head=0,tail=0;
 for(int y=0;y<h;y++)for(int x=0;x<w;x++){int k=y*w+x;pixels[k]=s.GetPixel(x,y).ToArgb();if(((uint)pixels[k]>>24)>=192){mask[k]=true;queue[tail++]=k;}}
 if(tail<1000)throw new Exception("Empty cabinet");
 while(head<tail){int k=queue[head++];if(dist[k]>=4)continue;int x=k%w,y=k/w;for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){int nx=x+dx,ny=y+dy;if(nx<0||ny<0||nx>=w||ny>=h)continue;int nk=ny*w+nx;if(!mask[nk]&&((uint)pixels[nk]>>24)>=8){mask[nk]=true;dist[nk]=(byte)(dist[k]+1);queue[tail++]=nk;}}}
 int x0=w,y0=h,x1=-1,y1=-1;
 for(int k=0;k<n;k++)if(mask[k]){int x=k%w,y=k/w;x0=Math.Min(x0,x);y0=Math.Min(y0,y);x1=Math.Max(x1,x);y1=Math.Max(y1,y);}
 using(Bitmap cropped=new Bitmap(x1-x0+5,y1-y0+5,System.Drawing.Imaging.PixelFormat.Format32bppArgb)){
 for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++)if(mask[y*w+x])cropped.SetPixel(x-x0+2,y-y0+2,Color.FromArgb(pixels[y*w+x]));
 cropped.Save(master,System.Drawing.Imaging.ImageFormat.Png);
 double scale=Math.Min((width-4.0)/cropped.Width,(height-4.0)/cropped.Height);
 int tw=(int)Math.Round(cropped.Width*scale),th=(int)Math.Round(cropped.Height*scale);
 using(Bitmap final=new Bitmap(width,height,System.Drawing.Imaging.PixelFormat.Format32bppArgb)){
 using(Graphics g=Graphics.FromImage(final))using(System.Drawing.Imaging.ImageAttributes attr=new System.Drawing.Imaging.ImageAttributes()){
 g.CompositingMode=System.Drawing.Drawing2D.CompositingMode.SourceCopy;
 g.InterpolationMode=System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
 g.PixelOffsetMode=System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
 attr.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
 g.DrawImage(cropped,new Rectangle((width-tw)/2,height-th-2,tw,th),0,0,cropped.Width,cropped.Height,GraphicsUnit.Pixel,attr);
 }
 final.Save(dest,System.Drawing.Imaging.ImageFormat.Png);
 }
 }
 }
 }
}
'@
Add-Type -TypeDefinition $taskCode -ReferencedAssemblies $taskRefs
$taskFiles=@(Get-Content -LiteralPath (Join-Path $Record '生成结果.json') -Raw | ConvertFrom-Json)
New-Item -ItemType Directory -Path (Join-Path $Record '成品'),(Join-Path $Record '高清素材'),(Join-Path $Record '生成原图') -Force | Out-Null
$taskReport=@()
foreach($taskF in $taskFiles){
 $taskRaw=Join-Path $Record "生成原图/$($taskF.Name)"
 $taskHigh=Join-Path $Record "高清素材/$($taskF.Name)"
 $taskDest=Join-Path $Record "成品/$($taskF.Name)"
 Copy-Item -LiteralPath $taskF.Generated -Destination $taskRaw
 [CabinetAsset]::Prepare($taskRaw,$taskHigh,$taskDest,$taskF.Width,$taskF.Height)
 $taskB=[System.Drawing.Bitmap]::new($taskDest)
 if($taskB.Width -ne $taskF.Width -or $taskB.Height -ne $taskF.Height -or $taskB.GetPixel(0,0).A -ne 0){throw 'Size or transparency failed'}
 $taskOpaque=0;$taskTransparent=0
 for($taskY=0;$taskY -lt $taskB.Height;$taskY++){for($taskX=0;$taskX -lt $taskB.Width;$taskX++){$taskA=$taskB.GetPixel($taskX,$taskY).A;if($taskA -eq 0){$taskTransparent++}elseif($taskA -eq 255){$taskOpaque++}}}
 if($taskOpaque -lt 100 -or $taskTransparent -lt 100){throw 'Invalid alpha'}
 $taskReport += [pscustomobject]@{Name=$taskF.Name;Width=$taskB.Width;Height=$taskB.Height;OpaquePixels=$taskOpaque;TransparentPixels=$taskTransparent;Sha256=(Get-FileHash -LiteralPath $taskDest).Hash}
 $taskB.Dispose()
}
$taskReport | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $Record '成品检查.json') -Encoding utf8
$taskPreview=[System.Drawing.Bitmap]::new(1020,420)
$taskG=[System.Drawing.Graphics]::FromImage($taskPreview)
$taskG.Clear([System.Drawing.Color]::FromArgb(64,67,73))
$taskG.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
$taskG.PixelOffsetMode=[System.Drawing.Drawing2D.PixelOffsetMode]::Half
$taskFont=[System.Drawing.Font]::new('Microsoft YaHei',14)
for($taskI=0;$taskI -lt 3;$taskI++){
 $taskF=$taskFiles[$taskI]
 $taskB=[System.Drawing.Bitmap]::new((Join-Path $Record "成品/$($taskF.Name)"))
 $taskScale=[Math]::Min(314.0/$taskB.Width,340.0/$taskB.Height)
 $taskW=[int][Math]::Round($taskB.Width*$taskScale);$taskH=[int][Math]::Round($taskB.Height*$taskScale)
 $taskG.DrawImage($taskB,[System.Drawing.Rectangle]::new([int]($taskI*340+(340-$taskW)/2),[int](355-$taskH),$taskW,$taskH))
 $taskG.DrawString(($taskF.Name -replace '.png$',''),$taskFont,[System.Drawing.Brushes]::White,[single]($taskI*340+80),[single]380)
 $taskB.Dispose()
}
$taskG.Dispose();$taskFont.Dispose()
$taskPreview.Save((Join-Path $Record '45度俯视总览.png'),[System.Drawing.Imaging.ImageFormat]::Png)
$taskPreview.Dispose()
$taskReport | ConvertTo-Json -Compress

