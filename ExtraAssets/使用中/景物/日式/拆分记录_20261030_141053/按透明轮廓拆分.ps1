param([string]$Root,[string]$Record)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$taskDir=Split-Path ([System.Drawing.Bitmap].Assembly.Location)
$taskRefs=@([System.Drawing.Bitmap].Assembly.Location,[System.Drawing.Rectangle].Assembly.Location,(Join-Path $PSHOME 'ref/System.Runtime.dll'),(Join-Path $taskDir 'System.Private.Windows.GdiPlus.dll'),(Join-Path $taskDir 'System.Private.Windows.Core.dll'))
$taskCode=@'
using System;using System.Drawing;
public static class SpriteContourSplit {
 public static string Run(string source,string output,string[] names){
 using(Bitmap s=new Bitmap(source)){
 int w=s.Width,h=s.Height,size=w*h;
 int[] pixels=new int[size],labels=new int[size],queue=new int[size],coreCounts=new int[1000];
 bool[] seen=new bool[size];byte[] distance=new byte[size];
 for(int y=0;y<h;y++)for(int x=0;x<w;x++){int k=y*w+x;pixels[k]=s.GetPixel(x,y).ToArgb();distance[k]=255;}
 int count=0,small=0,smallPixels=0;
 for(int k=0;k<size;k++){
 if(seen[k]||(uint)pixels[k]>>24<128)continue;
 int head=0,tail=1;queue[0]=k;seen[k]=true;
 while(head<tail){int p=queue[head++],x=p%w,y=p/w;
 for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){int nx=x+dx,ny=y+dy;if(nx<0||ny<0||nx>=w||ny>=h)continue;int nk=ny*w+nx;if(!seen[nk]&&((uint)pixels[nk]>>24)>=128){seen[nk]=true;queue[tail++]=nk;}}}
 if(tail<100){small++;smallPixels+=tail;continue;}
 count++;coreCounts[count]=tail;for(int j=0;j<tail;j++){labels[queue[j]]=count;distance[queue[j]]=0;}
 }
 if(count!=names.Length)throw new Exception("Unexpected asset count: "+count);
 int qHead=0,qTail=0;
 for(int k=0;k<size;k++)if(labels[k]>0)queue[qTail++]=k;
 while(qHead<qTail){int p=queue[qHead++],x=p%w,y=p/w;
 for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){int nx=x+dx,ny=y+dy;if(nx<0||ny<0||nx>=w||ny>=h)continue;int nk=ny*w+nx;if(labels[nk]==0&&((uint)pixels[nk]>>24)>=64){labels[nk]=labels[p];queue[qTail++]=nk;}}
 }
 qHead=0;qTail=0;
 for(int k=0;k<size;k++)if(labels[k]>0){distance[k]=0;queue[qTail++]=k;}
 while(qHead<qTail){int p=queue[qHead++];if(distance[p]>=8)continue;int x=p%w,y=p/w;
 for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){int nx=x+dx,ny=y+dy;if(nx<0||ny<0||nx>=w||ny>=h)continue;int nk=ny*w+nx;if(labels[nk]==0&&((uint)pixels[nk]>>24)>=8){labels[nk]=labels[p];distance[nk]=(byte)(distance[p]+1);queue[qTail++]=nk;}}
 }
 int[] x0=new int[count+1],y0=new int[count+1],x1=new int[count+1],y1=new int[count+1],visible=new int[count+1];
 for(int j=1;j<=count;j++){x0[j]=w;y0[j]=h;x1[j]=-1;y1[j]=-1;}
 for(int y=0;y<h;y++)for(int x=0;x<w;x++){int id=labels[y*w+x];if(id==0)continue;x0[id]=Math.Min(x0[id],x);y0[id]=Math.Min(y0[id],y);x1[id]=Math.Max(x1[id],x);y1[id]=Math.Max(y1[id],y);visible[id]++;}
 string items="[";long saved=0,foreignExcluded=0;
 for(int id=1;id<=count;id++){
 int bw=x1[id]-x0[id]+1,bh=y1[id]-y0[id]+1;string filename=id.ToString("D2")+"_"+names[id-1]+".png";
 string path=System.IO.Path.Combine(output,filename);
 using(Bitmap b=new Bitmap(bw+4,bh+4,System.Drawing.Imaging.PixelFormat.Format32bppArgb)){
 for(int y=y0[id];y<=y1[id];y++)for(int x=x0[id];x<=x1[id];x++){int k=y*w+x;if(labels[k]==id)b.SetPixel(x-x0[id]+2,y-y0[id]+2,Color.FromArgb(pixels[k]));else if(labels[k]>0)foreignExcluded++;}
 b.Save(path,System.Drawing.Imaging.ImageFormat.Png);
 }
 using(Bitmap b=new Bitmap(path)){
 if(b.Width!=bw+4||b.Height!=bh+4)throw new Exception("Output size differs");
 int actual=0;
 for(int y=0;y<b.Height;y++)for(int x=0;x<b.Width;x++){
 Color c=b.GetPixel(x,y);if(c.A==0)continue;
 int sx=x+x0[id]-2,sy=y+y0[id]-2;
 if(sx<0||sx>=w||sy<0||sy>=h||labels[sy*w+sx]!=id||c.ToArgb()!=pixels[sy*w+sx])throw new Exception("Mask or RGBA verification failed "+filename);
 actual++;
 }
 if(actual!=visible[id])throw new Exception("Lost contour pixels");
 }
 saved+=visible[id];if(id>1)items+=",";
 items+="{\"Id\":"+id+",\"Name\":\""+names[id-1]+"\",\"File\":\""+filename+"\",\"SourceX\":"+x0[id]+",\"SourceY\":"+y0[id]+",\"ContourWidth\":"+bw+",\"ContourHeight\":"+bh+",\"CanvasWidth\":"+(bw+4)+",\"CanvasHeight\":"+(bh+4)+",\"Padding\":2,\"CorePixels\":"+coreCounts[id]+",\"ExportedPixels\":"+visible[id]+"}";
 }
 int unassignedOpaque=0;
 for(int k=0;k<size;k++)if(((uint)pixels[k]>>24)>=128&&labels[k]==0)unassignedOpaque++;
 return "{\"Count\":"+count+",\"SeedAlpha\":128,\"EdgeAlphaMinimum\":8,\"EdgeGrowthMaximum\":8,\"MinCoreArea\":100,\"SavedPixels\":"+saved+",\"ForeignPixelsExcludedFromOverlappingBounds\":"+foreignExcluded+",\"SmallCoreComponents\":"+small+",\"SmallCorePixels\":"+smallPixels+",\"UnassignedOpaquePixels\":"+unassignedOpaque+",\"Assets\":"+items+"]}";
 }
 }
}
'@
Add-Type -TypeDefinition $taskCode -ReferencedAssemblies $taskRefs
$taskSource=Join-Path $Root '日式神社道具与家具素材表.png'
$taskOriginalHash=(Get-FileHash -LiteralPath $taskSource).Hash
$taskOutput=Join-Path $Root '独立素材'
if(-not(Test-Path -LiteralPath $taskOutput)){New-Item -ItemType Directory -Path $taskOutput | Out-Null}
$taskNames=Get-Content -LiteralPath (Join-Path $Record '素材名称.json') -Raw | ConvertFrom-Json
$taskResult=[SpriteContourSplit]::Run($taskSource,$taskOutput,[string[]]$taskNames)
$taskResult | Set-Content -LiteralPath (Join-Path $Record '拆分清单.json') -Encoding utf8
$taskData=$taskResult | ConvertFrom-Json
$taskData.Assets | Export-Csv -LiteralPath (Join-Path $Record '拆分清单.csv') -NoTypeInformation -Encoding utf8BOM
if((Get-FileHash -LiteralPath $taskSource).Hash -ne $taskOriginalHash){throw 'Source changed'}
$taskOverview=[System.Drawing.Bitmap]::new(1140,1800)
$taskG=[System.Drawing.Graphics]::FromImage($taskOverview)
$taskG.Clear([System.Drawing.Color]::FromArgb(61,65,70))
$taskG.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$taskG.PixelOffsetMode=[System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
$taskFont=[System.Drawing.Font]::new('Microsoft YaHei',10)
for($taskI=0;$taskI -lt $taskData.Assets.Count;$taskI++){
 $taskA=$taskData.Assets[$taskI]
 $taskB=[System.Drawing.Bitmap]::new((Join-Path $taskOutput $taskA.File))
 $taskScale=[Math]::Min(174.0/$taskB.Width,166.0/$taskB.Height)
 $taskW=[int][Math]::Round($taskB.Width*$taskScale);$taskH=[int][Math]::Round($taskB.Height*$taskScale)
 $taskX=($taskI%6)*190;$taskY=[int][Math]::Floor($taskI/6)*200
 $taskG.DrawImage($taskB,[System.Drawing.Rectangle]::new([int]($taskX+(190-$taskW)/2),[int]($taskY+(174-$taskH)/2),$taskW,$taskH))
 $taskG.DrawString(('{0:D2} {1}' -f $taskA.Id,$taskA.Name),$taskFont,[System.Drawing.Brushes]::White,[single]($taskX+7),[single]($taskY+178))
 $taskB.Dispose()
}
$taskG.Dispose();$taskFont.Dispose()
$taskOverview.Save((Join-Path $Record '独立素材总览.png'),[System.Drawing.Imaging.ImageFormat]::Png)
$taskOverview.Dispose()
$taskSourceHash=$taskOriginalHash
[pscustomobject]@{Source=$taskSource;SourceSha256=$taskSourceHash;SourceUnchanged=$true;Output=$taskOutput;Count=$taskData.Count;Pixels=$taskData.SavedPixels;UnassignedOpaquePixels=$taskData.UnassignedOpaquePixels;ExcludedNeighborPixels=$taskData.ForeignPixelsExcludedFromOverlappingBounds;RGBAValidated=$true} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $Record '验证结果.json') -Encoding utf8
Get-Content -LiteralPath (Join-Path $Record '验证结果.json') -Raw

