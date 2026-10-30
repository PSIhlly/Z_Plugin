param([string]$Root,[string]$Record)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$taskDir=Split-Path ([System.Drawing.Bitmap].Assembly.Location)
$taskRefs=@([System.Drawing.Bitmap].Assembly.Location,[System.Drawing.Rectangle].Assembly.Location,(Join-Path $PSHOME 'ref/System.Runtime.dll'),(Join-Path $taskDir 'System.Private.Windows.GdiPlus.dll'),(Join-Path $taskDir 'System.Private.Windows.Core.dll'))
$taskCode=@'
using System;using System.Drawing;
public static class TeaAsset {
 public static void Clean(string source,string dest){
 using(Bitmap s=new Bitmap(source)){
 int w=s.Width,h=s.Height,n=w*h;int[] colors=new int[n],q=new int[n],best=new int[n];bool[] seen=new bool[n];int bestCount=0;
 for(int y=0;y<h;y++)for(int x=0;x<w;x++)colors[y*w+x]=s.GetPixel(x,y).ToArgb();
 for(int k=0;k<n;k++){
 if(seen[k]||((uint)colors[k]>>24)<192)continue;int head=0,tail=1;q[0]=k;seen[k]=true;
 while(head<tail){int p=q[head++],x=p%w,y=p/w;for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){int nx=x+dx,ny=y+dy;if(nx<0||ny<0||nx>=w||ny>=h)continue;int nk=ny*w+nx;if(!seen[nk]&&((uint)colors[nk]>>24)>=192){seen[nk]=true;q[tail++]=nk;}}}
 if(tail>bestCount){bestCount=tail;Array.Copy(q,best,tail);}
 }
 if(bestCount<1000)throw new Exception("No opaque subject");
 bool[] mask=new bool[n];byte[] dist=new byte[n];int qh=0,qt=0;
 for(int j=0;j<bestCount;j++){int k=best[j];mask[k]=true;q[qt++]=k;}
 while(qh<qt){int k=q[qh++];if(dist[k]>=4)continue;int x=k%w,y=k/w;for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){int nx=x+dx,ny=y+dy;if(nx<0||ny<0||nx>=w||ny>=h)continue;int nk=ny*w+nx;if(!mask[nk]&&((uint)colors[nk]>>24)>=8){mask[nk]=true;dist[nk]=(byte)(dist[k]+1);q[qt++]=nk;}}}
 int x0=w,y0=h,x1=-1,y1=-1;for(int k=0;k<n;k++)if(mask[k]){int x=k%w,y=k/w;x0=Math.Min(x0,x);y0=Math.Min(y0,y);x1=Math.Max(x1,x);y1=Math.Max(y1,y);}
 using(Bitmap b=new Bitmap(x1-x0+5,y1-y0+5,System.Drawing.Imaging.PixelFormat.Format32bppArgb)){
 for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++)if(mask[y*w+x])b.SetPixel(x-x0+2,y-y0+2,Color.FromArgb(colors[y*w+x]));
 b.Save(dest,System.Drawing.Imaging.ImageFormat.Png);
 }
 }
 }
}
'@
Add-Type -TypeDefinition $taskCode -ReferencedAssemblies $taskRefs
$taskFiles=@(Get-Content -LiteralPath (Join-Path $Record '生成结果.json') -Raw | ConvertFrom-Json)
$taskOutput=Join-Path $Root '独立素材/22_茶桌分件_45度俯视'
if(Test-Path -LiteralPath $taskOutput){throw 'Output folder exists'}
New-Item -ItemType Directory -Path $taskOutput,(Join-Path $Record '生成原图'),(Join-Path $Record '高清素材') -Force | Out-Null
$taskItems=@()
foreach($taskF in $taskFiles){
 $taskName=$taskF.Name+'.png'
 $taskRaw=Join-Path (Join-Path $Record '生成原图') $taskName
 $taskHigh=Join-Path (Join-Path $Record '高清素材') $taskName
 Copy-Item -LiteralPath $taskF.Generated -Destination $taskRaw
 [TeaAsset]::Clean($taskRaw,$taskHigh)
 $taskS=[System.Drawing.Bitmap]::new($taskHigh)
 $taskH=[int][Math]::Round($taskS.Height*$taskF.Width/$taskS.Width)
 $taskB=[System.Drawing.Bitmap]::new(($taskF.Width+6),($taskH+6),[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
 $taskG=[System.Drawing.Graphics]::FromImage($taskB)
 $taskG.CompositingMode=[System.Drawing.Drawing2D.CompositingMode]::SourceCopy
 $taskG.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
 $taskG.PixelOffsetMode=[System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
 $taskAttr=[System.Drawing.Imaging.ImageAttributes]::new()
 $taskAttr.SetWrapMode([System.Drawing.Drawing2D.WrapMode]::TileFlipXY)
 $taskG.DrawImage($taskS,[System.Drawing.Rectangle]::new(3,3,$taskF.Width,$taskH),0,0,$taskS.Width,$taskS.Height,[System.Drawing.GraphicsUnit]::Pixel,$taskAttr)
 $taskG.Dispose();$taskAttr.Dispose();$taskS.Dispose()
 $taskDest=Join-Path $taskOutput $taskName
 $taskB.Save($taskDest,[System.Drawing.Imaging.ImageFormat]::Png)
 $taskB.Dispose()
 $taskCheck=[System.Drawing.Bitmap]::new($taskDest)
 if($taskCheck.GetPixel(0,0).A -ne 0){throw 'Background not transparent'}
 $taskItems += [pscustomobject]@{Name=$taskName;Path=$taskDest;Width=$taskCheck.Width;Height=$taskCheck.Height;Sha256=(Get-FileHash -LiteralPath $taskDest).Hash}
 $taskCheck.Dispose()
}
$taskPreview=[System.Drawing.Bitmap]::new(960,340)
$taskG=[System.Drawing.Graphics]::FromImage($taskPreview)
$taskG.Clear([System.Drawing.Color]::FromArgb(64,67,73))
$taskG.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
$taskG.PixelOffsetMode=[System.Drawing.Drawing2D.PixelOffsetMode]::Half
$taskFont=[System.Drawing.Font]::new('Microsoft YaHei',13)
for($taskI=0;$taskI -lt 4;$taskI++){
 $taskB=[System.Drawing.Bitmap]::new($taskItems[$taskI].Path)
 $taskScale=[Math]::Min(224.0/$taskB.Width,270.0/$taskB.Height)
 $taskW=[int][Math]::Round($taskB.Width*$taskScale);$taskH=[int][Math]::Round($taskB.Height*$taskScale)
 $taskG.DrawImage($taskB,[System.Drawing.Rectangle]::new([int]($taskI*240+(240-$taskW)/2),[int]((286-$taskH)/2),$taskW,$taskH))
 $taskG.DrawString(($taskItems[$taskI].Name -replace '_45度俯视.png',''),$taskFont,[System.Drawing.Brushes]::White,[single]($taskI*240+64),[single]300)
 $taskB.Dispose()
}
$taskG.Dispose();$taskFont.Dispose()
$taskPreview.Save((Join-Path $Record '分件总览.png'),[System.Drawing.Imaging.ImageFormat]::Png)
$taskPreview.Dispose()
if((Get-FileHash -LiteralPath (Join-Path $Root '独立素材/22_茶桌茶具与坐垫.png')).Hash -ne (Get-FileHash -LiteralPath (Join-Path $Record '参考原图.png')).Hash){throw 'Reference changed'}
$taskItems | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $Record '成品清单.json') -Encoding utf8
$taskItems | ConvertTo-Json -Compress

