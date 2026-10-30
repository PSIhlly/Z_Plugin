param([string]$Record)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$taskDir=Split-Path ([System.Drawing.Bitmap].Assembly.Location)
$taskRefs=@([System.Drawing.Bitmap].Assembly.Location,[System.Drawing.Rectangle].Assembly.Location,(Join-Path $PSHOME 'ref/System.Runtime.dll'),(Join-Path $taskDir 'System.Private.Windows.GdiPlus.dll'),(Join-Path $taskDir 'System.Private.Windows.Core.dll'))
$taskCode=@'
using System;
using System.Drawing;
public static class CornerRepair {
 static bool Purple(Color c){return c.A>=245&&c.R>=245&&c.G<=10&&c.B>=245;}
 static int Clamp(double v){return Math.Max(0,Math.Min(255,(int)Math.Round(v)));}
 static double Lum(Color c){return .299*c.R+.587*c.G+.114*c.B;}
 static bool Valid(int x,int y){return x>16&&y>16&&x<752&&y<752&&!(x>232&&x<536&&y>232&&y<536);}
 public static string Repair(string source,string generated,string dest){
 using(Bitmap s=new Bitmap(source)) using(Bitmap raw=new Bitmap(generated)) using(Bitmap g=new Bitmap(s.Width,s.Height)) using(Bitmap o=new Bitmap(s.Width,s.Height,System.Drawing.Imaging.PixelFormat.Format32bppArgb)){
 int w=s.Width,h=s.Height;
 for(int y=0;y<h;y++)for(int x=0;x<w;x++)o.SetPixel(x,y,s.GetPixel(x,y));
 for(int y=0;y<h;y++)for(int x=0;x<w;x++)g.SetPixel(x,y,raw.GetPixel(Math.Min(raw.Width-1,(int)((x+.5)*raw.Width/w)),Math.Min(raw.Height-1,(int)((y+.5)*raw.Height/h))));
 int bestX=0,bestY=0;double best=-1;
 for(int dy=-10;dy<=10;dy++)for(int dx=-10;dx<=10;dx++){
 double dot=0,sa=0,ga=0;
 for(int y=24;y<h-24;y+=12)for(int x=24;x<w-24;x+=12){
 if(!Valid(x,y)||s.GetPixel(x,y).A<250||g.GetPixel(x+dx,y+dy).A<250)continue;
 double ax=Lum(s.GetPixel(x+3,y))-Lum(s.GetPixel(x-3,y)),ay=Lum(s.GetPixel(x,y+3))-Lum(s.GetPixel(x,y-3));
 double bx=Lum(g.GetPixel(x+dx+3,y+dy))-Lum(g.GetPixel(x+dx-3,y+dy)),by=Lum(g.GetPixel(x+dx,y+dy+3))-Lum(g.GetPixel(x+dx,y+dy-3));
 dot+=ax*bx+ay*by;sa+=ax*ax+ay*ay;ga+=bx*bx+by*by;
 }
 double score=dot/Math.Sqrt(Math.Max(1,sa*ga));if(score>best){best=score;bestX=dx;bestY=dy;}
 }
 double[] sumS=new double[3],sumG=new double[3],sumGG=new double[3],sumGS=new double[3];int n=0;
 for(int y=24;y<h-24;y+=6)for(int x=24;x<w-24;x+=6){
 if(!Valid(x,y))continue;Color a=s.GetPixel(x,y),b=g.GetPixel(x+bestX,y+bestY);if(a.A<250||b.A<250)continue;
 double[] ac={a.R,a.G,a.B},bc={b.R,b.G,b.B};for(int k=0;k<3;k++){sumS[k]+=ac[k];sumG[k]+=bc[k];sumGG[k]+=bc[k]*bc[k];sumGS[k]+=bc[k]*ac[k];}n++;
 }
 double[] slope=new double[3],offset=new double[3];
 for(int k=0;k<3;k++){slope[k]=Math.Max(.5,Math.Min(1.5,(sumGS[k]-sumG[k]*sumS[k]/n)/Math.Max(1,sumGG[k]-sumG[k]*sumG[k]/n)));offset[k]=(sumS[k]-slope[k]*sumG[k])/n;}
 int mask=0,changed=0,remaining=0,outside=0;
 for(int y=0;y<h;y++)for(int x=0;x<w;x++){
 if(!Purple(s.GetPixel(x,y)))continue;mask++;
 int gx=Math.Max(0,Math.Min(w-1,(x/4)*4+2+bestX)),gy=Math.Max(0,Math.Min(h-1,(y/4)*4+2+bestY));
 Color c=g.GetPixel(gx,gy);double r=slope[0]*c.R+offset[0],gr=slope[1]*c.G+offset[1],b=slope[2]*c.B+offset[2],a=c.A;
 int dist=Math.Min(Math.Min(x-256,511-x),Math.Min(y-256,511-y));
 if(dist<24){
 int ex=x,ey=y;
 if(dist==x-256)ex=255;else if(dist==511-x)ex=512;else if(dist==y-256)ey=255;else ey=512;
 Color edgeS=s.GetPixel(ex,ey),edgeG=g.GetPixel(Math.Max(0,Math.Min(w-1,ex+bestX)),Math.Max(0,Math.Min(h-1,ey+bestY)));
 double weight=Math.Pow((24.0-dist)/24,2);
 if(edgeS.A>=200&&edgeG.A>=200){
 r+=(edgeS.R-(slope[0]*edgeG.R+offset[0]))*weight;
 gr+=(edgeS.G-(slope[1]*edgeG.G+offset[1]))*weight;
 b+=(edgeS.B-(slope[2]*edgeG.B+offset[2]))*weight;
 }
 a+=(edgeS.A-edgeG.A)*weight;
 if(c.A<128&&a>0&&edgeS.A>0){r=edgeS.R;gr=edgeS.G;b=edgeS.B;}
 }
 if(b-r>45){double l=.3*r+.6*gr+.1*b;r=l-8;gr=l;b=l+5;}
 o.SetPixel(x,y,Color.FromArgb(Clamp(a),Clamp(r),Clamp(gr),Clamp(b)));
 }
 bool[] seen=new bool[w*h];int[] queue=new int[w*h];
 for(int cy=256;cy<512;cy++)for(int cx=256;cx<512;cx++){
 int start=cy*w+cx;if(seen[start]||o.GetPixel(cx,cy).A<16)continue;
 int head=0,tail=1;queue[0]=start;seen[start]=true;bool connected=false;
 while(head<tail){int p=queue[head++],px=p%w,py=p/w;if(!Purple(s.GetPixel(px,py))&&o.GetPixel(px,py).A>=128)connected=true;
 for(int d=0;d<4;d++){int nx=px+(d==0?-1:d==1?1:0),ny=py+(d==2?-1:d==3?1:0);if(nx<0||nx>=w||ny<0||ny>=h)continue;int np=ny*w+nx;if(!seen[np]&&o.GetPixel(nx,ny).A>=16){seen[np]=true;queue[tail++]=np;}}
 }
 if(!connected&&tail<128)for(int j=0;j<tail;j++){int px=queue[j]%w,py=queue[j]/w;if(Purple(s.GetPixel(px,py)))o.SetPixel(px,py,Color.FromArgb(0,0,0,0));}
 }
 for(int y=0;y<h;y++)for(int x=0;x<w;x++){Color oc=o.GetPixel(x,y),sc=s.GetPixel(x,y);if(oc.ToArgb()!=sc.ToArgb()){changed++;if(!Purple(sc))outside++;}if(Purple(oc))remaining++;}
 if(outside!=0||remaining!=0||mask<60000)throw new Exception("Mask validation failed: mask="+mask+", remaining="+remaining+", outside="+outside);
 o.Save(dest,System.Drawing.Imaging.ImageFormat.Png);
 return "{\"MaskPixels\":"+mask+",\"ChangedPixels\":"+changed+",\"OutsideDifference\":"+outside+",\"RemainingPurple\":"+remaining+",\"OffsetX\":"+bestX+",\"OffsetY\":"+bestY+",\"AlignmentScore\":"+best.ToString(System.Globalization.CultureInfo.InvariantCulture)+"}";
 }
 }
}
'@
Add-Type -TypeDefinition $taskCode -ReferencedAssemblies $taskRefs
$taskFiles=@(Get-Content -LiteralPath (Join-Path $Record '生成结果.json') -Raw | ConvertFrom-Json)
New-Item -ItemType Directory -Path (Join-Path $Record '成品'),(Join-Path $Record '生成原图') -Force | Out-Null
$taskStats=@()
foreach($taskF in $taskFiles){
 $taskN=$taskF.Index
 $taskRaw=Join-Path $Record "生成原图/$taskN.png"
 Copy-Item -LiteralPath $taskF.Generated -Destination $taskRaw
 $taskDst=Join-Path $Record "成品/$taskN.png"
 $taskStat=[CornerRepair]::Repair((Join-Path $Record "原图备份/$taskN.png"),$taskRaw,$taskDst) | ConvertFrom-Json
 $taskStat | Add-Member -NotePropertyName Index -NotePropertyValue $taskN
 $taskStats+=$taskStat
}
$taskStats | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $Record '区域像素检查.json') -Encoding utf8
$taskStats | ConvertTo-Json -Compress
$taskOverview=[System.Drawing.Bitmap]::new(768,768)
$taskG=[System.Drawing.Graphics]::FromImage($taskOverview)
$taskG.Clear([System.Drawing.Color]::FromArgb(55,57,61))
$taskG.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
$taskG.PixelOffsetMode=[System.Drawing.Drawing2D.PixelOffsetMode]::Half
for($taskI=0;$taskI -lt 4;$taskI++){$taskB=[System.Drawing.Bitmap]::new((Join-Path $Record "成品/$($taskI+22).png"));$taskG.DrawImage($taskB,[System.Drawing.Rectangle]::new(($taskI%2)*384,[int][Math]::Floor($taskI/2)*384,384,384));$taskB.Dispose()}
$taskG.Dispose()
$taskOverview.Save((Join-Path $Record '修复总览.png'),[System.Drawing.Imaging.ImageFormat]::Png)
$taskOverview.Dispose()

