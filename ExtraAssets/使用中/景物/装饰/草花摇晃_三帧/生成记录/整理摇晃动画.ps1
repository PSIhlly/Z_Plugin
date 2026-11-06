param([string]$Output)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$taskDir=Split-Path ([System.Drawing.Bitmap].Assembly.Location)
$taskRefs=@([System.Drawing.Bitmap].Assembly.Location,[System.Drawing.Rectangle].Assembly.Location,(Join-Path $PSHOME 'ref/System.Runtime.dll'),(Join-Path $taskDir 'System.Private.Windows.GdiPlus.dll'),(Join-Path $taskDir 'System.Private.Windows.Core.dll'))
$taskCode=@'
using System;using System.Drawing;using System.IO;
public static class PlantSwayFinish {
static void Code(MemoryStream m,int code,int len,ref int bits,ref int value){value|=code<<bits;bits+=len;while(bits>=8){m.WriteByte((byte)(value&255));value>>=8;bits-=8;}}
static void Gif(string path,int[][] frames,Color[] pal,int w,int h,int scale){
int depth=2;while((1<<depth)<pal.Length)depth++;int clear=1<<depth;
using(BinaryWriter b=new BinaryWriter(File.Create(path))){b.Write(System.Text.Encoding.ASCII.GetBytes("GIF89a"));b.Write((ushort)(w*scale));b.Write((ushort)(h*scale));b.Write((byte)(0xF0|(depth-1)));b.Write(new byte[]{0,0});for(int i=0;i<clear;i++){Color c=i<pal.Length?pal[i]:Color.Black;b.Write(c.R);b.Write(c.G);b.Write(c.B);}
b.Write(new byte[]{0x21,0xff,11});b.Write(System.Text.Encoding.ASCII.GetBytes("NETSCAPE2.0"));b.Write(new byte[]{3,1,0,0,0});
foreach(int f in new int[]{0,1,2,1}){b.Write(new byte[]{0x21,0xf9,4,9,22,0,0,0,0x2c});b.Write((ushort)0);b.Write((ushort)0);b.Write((ushort)(w*scale));b.Write((ushort)(h*scale));b.Write((byte)0);b.Write((byte)depth);
using(MemoryStream m=new MemoryStream()){int bits=0,value=0;for(int y=0;y<h*scale;y++)for(int x=0;x<w*scale;x++){Code(m,clear,depth+1,ref bits,ref value);Code(m,frames[f][(y/scale)*w+x/scale],depth+1,ref bits,ref value);}Code(m,clear+1,depth+1,ref bits,ref value);if(bits>0)m.WriteByte((byte)value);byte[] data=m.ToArray();for(int i=0;i<data.Length;i+=255){int len=Math.Min(255,data.Length-i);b.Write((byte)len);b.Write(data,i,len);}b.Write((byte)0);}}
b.Write((byte)0x3b);}}
public static string Run(string source,string[] generated,string output,string name,string record){
using(Bitmap s=new Bitmap(source)){
int w=s.Width,h=s.Height,n=w*h;Color[] src=new Color[n],tmp=new Color[n+1];tmp[0]=Color.FromArgb(0,0,0,0);int pc=1,x0=w,x1=-1,y0=h,y1=-1;
for(int y=0;y<h;y++)for(int x=0;x<w;x++){Color c=s.GetPixel(x,y);src[y*w+x]=c;if(c.A<128)continue;x0=Math.Min(x0,x);x1=Math.Max(x1,x);y0=Math.Min(y0,y);y1=Math.Max(y1,y);bool found=false;for(int k=1;k<pc;k++)if(tmp[k].ToArgb()==Color.FromArgb(255,c).ToArgb())found=true;if(!found)tmp[pc++]=Color.FromArgb(255,c);}
Color[] pal=new Color[pc];Array.Copy(tmp,pal,pc);int[][] indexed=new int[3][];string rows="[";
for(int f=0;f<3;f++){Color[] frame=new Color[n];indexed[f]=new int[n];for(int i=0;i<n;i++)frame[i]=Color.FromArgb(0,0,0,0);
if(f==1)Array.Copy(src,frame,n);
else using(Bitmap g=new Bitmap(generated[f==0?0:1])){
int ax=g.Width,bx=-1,ay=g.Height,by=-1;for(int y=0;y<g.Height;y++)for(int x=0;x<g.Width;x++)if(g.GetPixel(x,y).A>=160){ax=Math.Min(ax,x);bx=Math.Max(bx,x);ay=Math.Min(ay,y);by=Math.Max(by,y);}if(bx<ax)throw new Exception("Empty pose");
for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++){
Color c=g.GetPixel(Math.Min(bx,ax+(int)((x-x0+.5)*(bx-ax+1)/(x1-x0+1))),Math.Min(by,ay+(int)((y-y0+.5)*(by-ay+1)/(y1-y0+1))));if(c.A<160)continue;
int shift=(y1-y)/(double)Math.Max(1,y1-y0)>.42?(f==0?-1:1):0,xx=x+shift;if(xx<0||xx>=w)continue;bool near=false;
for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){int nx=xx+dx,ny=y+dy;if(nx>=0&&ny>=0&&nx<w&&ny<h&&src[ny*w+nx].A>=128)near=true;}if(!near)continue;
int best=1;double score=double.MaxValue;for(int k=1;k<pc;k++){double d=2*Math.Pow(c.R-pal[k].R,2)+4*Math.Pow(c.G-pal[k].G,2)+Math.Pow(c.B-pal[k].B,2);if(d<score){score=d;best=k;}}frame[y*w+xx]=pal[best];
}
for(int y=Math.Max(0,y1-1);y<h;y++)for(int x=0;x<w;x++)frame[y*w+x]=src[y*w+x];
}
int changed=0,visible=0;using(Bitmap b=new Bitmap(w,h,System.Drawing.Imaging.PixelFormat.Format32bppArgb)){
for(int y=0;y<h;y++)for(int x=0;x<w;x++){int p=y*w+x;Color c=frame[p];b.SetPixel(x,y,c);if(c.ToArgb()!=src[p].ToArgb())changed++;if(c.A<128)continue;visible++;for(int k=1;k<pc;k++)if(Color.FromArgb(255,c).ToArgb()==pal[k].ToArgb()){indexed[f][p]=k;break;}}
string file=name+"_"+(f+1).ToString("D2")+".png";b.Save(Path.Combine(output,file),System.Drawing.Imaging.ImageFormat.Png);
using(Bitmap check=new Bitmap(Path.Combine(output,file))){if(check.Width!=w||check.Height!=h)throw new Exception("Size");for(int y=Math.Max(0,y1-1);y<h;y++)for(int x=0;x<w;x++)if(check.GetPixel(x,y).ToArgb()!=src[y*w+x].ToArgb())throw new Exception("Root moved");}
if(f>0)rows+=",";rows+="{\"File\":\""+file+"\",\"Width\":"+w+",\"Height\":"+h+",\"VisiblePixels\":"+visible+",\"ChangedPixels\":"+changed+",\"RootFixed\":true}";
}}
using(Bitmap p=new Bitmap(w*24,h*8,System.Drawing.Imaging.PixelFormat.Format32bppArgb)){for(int y=0;y<h*8;y++)for(int x=0;x<w*24;x++)p.SetPixel(x,y,pal[indexed[x/(w*8)][(y/8)*w+(x%(w*8))/8]]);p.Save(Path.Combine(record,name+"_三帧预览.png"),System.Drawing.Imaging.ImageFormat.Png);}
Gif(Path.Combine(output,name+"_摇晃.gif"),indexed,pal,w,h,1);Gif(Path.Combine(record,name+"_循环放大预览.gif"),indexed,pal,w,h,12);
using(Image gif=Image.FromFile(Path.Combine(output,name+"_摇晃.gif"))){if(gif.Width!=w||gif.Height!=h||gif.GetFrameCount(System.Drawing.Imaging.FrameDimension.Time)!=4)throw new Exception("GIF verification");}
return rows+"]";
}}
}
'@
Add-Type -TypeDefinition $taskCode -ReferencedAssemblies $taskRefs
$taskJobs=Get-Content -LiteralPath (Join-Path $Output '生成记录/生成结果.json') -Raw|ConvertFrom-Json
$taskRows=@()
foreach($taskName in @('草1','草2','花1','花2','花3','花4')){
 $taskDir=Join-Path $Output $taskName
 New-Item -ItemType Directory -Force -Path $taskDir|Out-Null
 $taskA=$taskJobs|Where-Object { $_.Name -eq $taskName -and $_.Frame -eq 1 }
 $taskB=$taskJobs|Where-Object { $_.Name -eq $taskName -and $_.Frame -eq 3 }
 $taskSource=Join-Path $Output "生成记录/原图备份/$taskName.png"
 $taskResult=[PlantSwayFinish]::Run($taskSource,[string[]]@($taskA.Path,$taskB.Path),$taskDir,$taskName,(Join-Path $Output '生成记录'))|ConvertFrom-Json
 foreach($taskJob in @($taskA,$taskB)){Copy-Item -LiteralPath $taskJob.Path -Destination (Join-Path $Output ('生成记录/{0}_{1}_生成原图.png' -f $taskName,$taskJob.Frame))}
 $taskUnique=@(Get-ChildItem -LiteralPath $taskDir -Filter '*.png' -File|ForEach-Object {(Get-FileHash -LiteralPath $_.FullName).Hash}|Select-Object -Unique)
 if($taskUnique.Count -ne 3){throw "Duplicate poses: $taskName"}
 $taskStatic=Join-Path (Split-Path $Output) "$taskName.png"
 if((Get-FileHash -LiteralPath $taskSource).Hash -ne (Get-FileHash -LiteralPath $taskStatic).Hash){throw 'Original changed'}
 $taskRows+=[pscustomobject]@{Name=$taskName;OriginalUnchanged=$true;UniqueFrames=3;Frames=$taskResult}
}
$taskRows|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $Output '生成记录/尺寸根部与原图验证.json') -Encoding utf8
'6组草花摇晃，每组3张16x16透明PNG。01左摆、02原姿态、03右摆，播放01→02→03→02循环，每步0.22秒。GIF由3张图片组成4拍循环。原始静态图片不变；根部两行固定；保持原色板。'|Set-Content -LiteralPath (Join-Path $Output '使用说明.txt') -Encoding utf8
$taskRows|ConvertTo-Json -Depth 6

