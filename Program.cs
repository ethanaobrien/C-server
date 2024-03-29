﻿using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace WebServer
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new WebServer());
        }
    }
}

public class Updater
{
    private double version;
    private string versionURL = "https://raw.githubusercontent.com/ethanaobrien/C-server/main/version";
    public Updater(double version)
    {
        this.version = version;
    }
    public async void check4Updates(Action<Double> callback)
    {
        double current_version;
        try
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(versionURL);
            current_version = Double.Parse(await response.Content.ReadAsStringAsync());
        }
        catch (Exception e)
        {
            return;
        }
        if (current_version > this.version)
        {
            callback(current_version);
        }
    }
}

public class Server
{
    private int readChunkSize = 1024 * 1024 * 8;
    private int writeChunkSize = 1024;
    private Socket listener = null;
    private Thread mainThread;
    private struct SET
    {
        public int port;
        public string mainPath;
        public bool allowDelete;
        public bool allowPut;
        public bool cors;
        public bool index;
        public bool directoryListing;
        public bool LocalNetwork;
        public SET(int port, string mainPath, bool allowPut, bool allowDelete, bool cors, bool index, bool directoryListing, bool LocalNetwork)
        {
            this.port = port;
            this.mainPath = mainPath;
            this.allowPut = allowPut;
            this.allowDelete = allowDelete;
            this.cors = cors;
            this.index = index;
            this.directoryListing = directoryListing;
            this.LocalNetwork = LocalNetwork;
        }
    }
    private SET Settings;
    public Server()
    {
        this.Settings = new SET(8080, "C:", false, false, false, false, true, false);
        this.mainThread = new Thread(ServerMain);
        this.mainThread.Start();
    }


    public Server(string path, int port)
    {
        this.Settings = new SET(port, path, false, false, false, false, true, false);
        this.mainThread = new Thread(ServerMain);
        this.mainThread.Start();
    }
    public Server(string path, int port, bool allowPut, bool allowDelete, bool cors, bool index, bool directoryListing, bool LocalNetwork)
    {
        this.Settings = new SET(port, path, allowPut, allowDelete, cors, index, directoryListing, LocalNetwork);
        this.mainThread = new Thread(ServerMain);
        this.mainThread.Start();
    }
    bool running = true;
    public void setMainPath(string path)
    {
        this.Settings.mainPath = path;
    }
    public void setDelete(bool allow)
    {
        this.Settings.allowDelete = allow;
    }
    public void setDirectory(bool allow)
    {
        this.Settings.directoryListing = allow;
    }
    public void setLocalNetwork(bool yes)
    {
        this.Settings.LocalNetwork = yes;
    }
    public void setCors(bool set)
    {
        this.Settings.cors = set;
    }
    public void setPut(bool allow)
    {
        this.Settings.allowPut = allow;
    }
    public void setIndex(bool allow)
    {
        this.Settings.index = allow;
    }
    public void Terminate()
    {
        if (this.listener == null) return;
        try
        {
            running = false;
            this.listener.Shutdown(SocketShutdown.Both);
        }
        catch (Exception e) { }
        try
        {
            this.listener.Close();
        }
        catch (Exception e) { }
        try
        {
            this.listener.Dispose();
        }
        catch (Exception e) { }
        try
        {
            this.listener = null;
        }
        catch (Exception e) { }
        try
        {
            this.mainThread.Abort();
        }
        catch (Exception e) { }
    }
    private void ServerMain()
    {
        try
        {
            string ListenHost = this.Settings.LocalNetwork ? "0.0.0.0" : "127.0.0.1";
            IPAddress ipAddress = IPAddress.Parse(ListenHost);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, this.Settings.port);
            this.listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.listener.Bind(localEndPoint);
            this.listener.Listen(100); //connection limit
            Console.WriteLine("Listening on http://{0}:{1}", ListenHost, this.Settings.port);
            while (running)
            {
                try
                {
                    Socket handler = this.listener.Accept();
                    Thread t = new Thread(onRequest);
                    t.Start(handler);
                }
                catch (Exception e)
                {
                    break;
                }
            }
            this.listener.Shutdown(SocketShutdown.Both);
            this.listener.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
    public string[] GetURLs()
    {
        if (this.Settings.LocalNetwork)
        {
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] addr = ipEntry.AddressList;
            string[] rv = new string[addr.Length];
            rv[0] = "127.0.0.1";
            for (int i = 0, j = 1; i < addr.Length; i++)
            {
                if (addr[i].AddressFamily == AddressFamily.InterNetwork)
                {
                    rv[j] = addr[i].ToString();
                    j++;
                }
            }
            return rv;
        }
        else
        {
            return new string[] { "127.0.0.1" };
        }
    }
    private void onRequest(Object obj)
    {
        Socket handler = (Socket)obj;
        try
        {
            while (running)
            {
                bool consumed = false;
                string data = "";
                while (!consumed)
                {
                    byte[] bytes = new byte[1];
                    int bytesRec = handler.Receive(bytes);
                    if (bytesRec == 0) break;
                    data += Encoding.UTF8.GetString(bytes, 0, bytesRec);
                    consumed = (data.IndexOf("\r\n\r\n") != -1);
                }
                if (!consumed)
                    break;
                //Console.WriteLine("Text received : {0}", data);
                string url = Uri.UnescapeDataString(data.Split(' ')[1].Split('?')[0]);
                string method = data.Split(' ')[0];
                if (this.Settings.mainPath.Length == 0)
                {
                    byte[] msg = Encoding.UTF8.GetBytes("Path to serve not set!");
                    writeHeader(handler, 500, "Internal Server Error", (long)msg.Length, "", "");
                    handler.Send(msg);
                    continue;
                }
                string path = this.Settings.mainPath + url;
                if (!url.EndsWith("/") && Directory.Exists(path))
                {
                    writeHeader(handler, 301, "Moved permanently", 0, "", "Location: " + url + "/\r\n");
                    continue;
                }
                if (url.EndsWith("/") && File.Exists(path.Substring(0, path.Length - 1)))
                {
                    writeHeader(handler, 301, "Moved permanently", 0, "", "Location: " + url.Substring(0, url.Length - 1) + "\r\n");
                    continue;
                }
                Console.WriteLine("Request {0} {1}", method, url);
                if (method.Equals("HEAD") || method.Equals("GET"))
                {
                    GetHead(handler, method, url, path, data);
                }
                else if (method.Equals("PUT") && this.Settings.allowPut)
                {
                    put(handler, path, data);
                }
                else if (method.Equals("DELETE") && this.Settings.allowDelete)
                {
                    del(handler, path);
                }
                else
                {
                    byte[] msg = Encoding.UTF8.GetBytes("405 - Method not allowed");
                    string extraHeader = "Allow: GET, HEAD";
                    if (this.Settings.allowPut) extraHeader += ", PUT";
                    if (this.Settings.allowDelete) extraHeader += ", DELETE";
                    writeHeader(handler, 405, "Method Not Allowed", (long)msg.Length, "", extraHeader + "\r\n");
                    handler.Send(msg);
                }
                //System.Windows.Forms.MessageBox.Show(url);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: {0}", e.ToString());
        }
        handler.Shutdown(SocketShutdown.Both);
        handler.Close();
    }
    private void writeHeader(Socket handler, int httpCode, string code, long cl, string ct, string extra)
    {
        string header = "HTTP/1.1 " + httpCode + " " + code + "\r\nAccept-Ranges: bytes\r\nConnection: keep-alive\r\n";
        if (ct.Length > 0)
        {
            header += "Content-type: " + ct + "\r\n";
        }
        if (extra.Length > 0)
        {
            header += extra;
        }
        if (this.Settings.cors)
        {
            header += "Access-Control-Allow-Origin: *\r\n";
        }
        header += "Content-Length: " + cl + "\r\n\r\n";
        byte[] msg = Encoding.UTF8.GetBytes(header);
        handler.Send(msg);
    }
    private string getMime(string fileName)
    {
        string[] w = fileName.Split('.');
        string ext = w[w.Length - 1];
        string delim = "," + ext + ":";
        int i = mimetypes.IndexOf(delim);
        if (i != -1)
        {
            return mimetypes.Substring(i + delim.Length).Split(',')[0];
        }
        return "";
    }
    private void error(Socket handler)
    {
        try
        {
            byte[] msg = Encoding.UTF8.GetBytes("500 - Internal Server Error");
            writeHeader(handler, 500, "INTERNAL SERVER ERROR", (long)msg.Length, "", "");
            handler.Send(msg);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error2: {0}", e.ToString());
        }
    }
    private void GetHead(Socket handler, string method, string url, string inPath, string data)
    {
        string path = inPath;
        try
        {
            if (Directory.Exists(path))
            {
                Boolean index = false;
                string[] Files = System.IO.Directory.GetFileSystemEntries(path);
                if (this.Settings.index)
                {
                    foreach (string sFilez in Files)
                    {
                        string fileName = sFilez.Split('/')[sFilez.Split('/').Length - 1];
                        if ((fileName.ToLower().Equals("index.html") || fileName.ToLower().Equals("index.htm")) && File.Exists(path + fileName))
                        {
                            Console.WriteLine(fileName);
                            index = true;
                            path += fileName;
                            break;
                        }
                    }
                }
                if (!index)
                {
                    if (!this.Settings.directoryListing)
                    {
                        byte[] msg3 = Encoding.UTF8.GetBytes("404 - File Not Found");
                        writeHeader(handler, 404, "NOT FOUND", (long)msg3.Length, "", "");
                        handler.Send(msg3);
                        return;
                    }
                    string resp = directoryListingTemplate + "<script>start(decodeURI(window.location.pathname));</script>";
                    if (!url.Equals("/"))
                    {
                        resp += "<script>onHasParentDirectory();</script>";
                    }
                    foreach (string sFile in Files)
                    {
                        string[] s = sFile.Split('/', '\\');
                        string fileName = s[s.Length - 1];
                        resp += "\n<script>addRow(\"" + fileName + "\", \"" + fileName + "\", " + (Directory.Exists(sFile) ? 1 : 0) + ", '', '', '', '');</script>";
                    }
                    byte[] msg2 = Encoding.UTF8.GetBytes(resp);
                    writeHeader(handler, 200, "OK", (long)msg2.Length, "text/html; charset=utf-8", "");
                    if (method.Equals("HEAD"))
                    {
                        return;
                    }
                    handler.Send(msg2);
                    return;
                }
            }
            if (File.Exists(path))
            {
                string range = "";
                string d = data.ToLower();
                string delim = "\r\nrange: ";
                int i = d.IndexOf(delim);
                if (i != -1)
                {
                    range = data.Substring(i + delim.Length).Split('\r')[0];
                }
                var file = new FileInfo(path);

                string rheader = "";
                long fileOffset = 0, fileEndOffset = file.Length, len = file.Length + 1;
                long cl = file.Length;
                int code = 200;
                if (range.Length > 0)
                {
                    string ran = range.Split('=')[1];
                    string[] rparts = ran.Split('-');
                    if (rparts[1].Length == 0)
                    {
                        fileOffset = Int32.Parse(rparts[0]);
                        fileEndOffset = file.Length;
                        cl = len - fileOffset - 1;
                        rheader = "Content-Range: bytes " + fileOffset + "-" + (len - 2) + "/" + (len - 1) + "\r\n";
                        code = (fileOffset == 0) ? 200 : 206;
                    }
                    else
                    {
                        fileOffset = Int32.Parse(rparts[0]);
                        fileEndOffset = Int32.Parse(rparts[1]);
                        cl = fileEndOffset - fileOffset + 1;
                        rheader = "Content-Range: bytes " + fileOffset + "-" + (fileEndOffset) + "/" + (len - 1) + "\r\n";
                        code = 206;
                    }
                }
                writeHeader(handler, code, "OK", cl, getMime(path), rheader);
                if (method.Equals("HEAD"))
                {
                    return;
                }
                FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                BinaryReader reader = new BinaryReader(stream);
                reader.BaseStream.Position = fileOffset;
                long readLen = 0;
                while (readLen <= cl)
                {
                    int a = this.readChunkSize;
                    if (cl - readLen < this.readChunkSize)
                    {
                        a = (int)(cl - readLen);
                    }
                    if (a == 0) break;
                    readLen += a;
                    byte[] res = reader.ReadBytes(a);
                    try
                    {
                        handler.Send(res);
                    }
                    catch (Exception e)
                    {
                        //Console.WriteLine("Errorr: {0}", e.ToString());
                        reader.Close();
                        stream.Close();
                        return;
                    }
                }
                reader.Close();
                stream.Close();
                return;
            }
            byte[] msg = Encoding.UTF8.GetBytes("404 - File Not Found");
            writeHeader(handler, 404, "NOT FOUND", (long)msg.Length, "", "");
            handler.Send(msg);
        }
        catch (Exception e)
        {
            error(handler);
            Console.WriteLine("Error: {0}", e.ToString());
        }
    }
    private void put(Socket handler, string path, string data)
    {
        try
        {
            long cl = 0;
            string d = data.ToLower();
            string delim = "\r\ncontent-length: ";
            int i = d.IndexOf(delim);
            if (i == -1)
            {
                byte[] msg = Encoding.UTF8.GetBytes("400 - Bad Request");
                writeHeader(handler, 400, "Bad Request", (long)msg.Length, "", "");
                handler.Send(msg);
                return;
            }
            cl = Int64.Parse(data.Substring(i + delim.Length).Split('\r')[0]);
            if (File.Exists(path) || Directory.Exists(path))
            {
                byte[] msg = Encoding.UTF8.GetBytes("400 - Bad Request");
                writeHeader(handler, 400, "Bad Request", (long)msg.Length, "", "");
                handler.Send(msg);
                return;
            }
            string folder = Path.GetDirectoryName(path);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            FileStream fs = File.Create(path);
            long written = 0;
            while (written < cl)
            {
                int a = this.writeChunkSize;
                if ((int)(cl - written) < this.writeChunkSize)
                {
                    a = (int)(cl - written);
                }
                written += (long)a;
                byte[] bytes = new byte[a];
                int qwe = handler.Receive(bytes);
                if (qwe == 0) break;
                fs.Write(bytes, 0, a);
            }
            fs.Close();
            writeHeader(handler, 201, "Created", 0, "", "");
        }
        catch (Exception e)
        {
            error(handler);
            Console.WriteLine("Error: {0}", e.ToString());
        }
    }
    private void del(Socket handler, string path)
    {
        try
        {
            File.Delete(path);
            writeHeader(handler, 204, "No Content", 0, "", "");
        }
        catch (Exception e)
        {
            error(handler);
            Console.WriteLine("Error: {0}", e.ToString());
        }
    }
    private const string mimetypes = ",123:application/vnd.lotus-1-2-3,3dml:text/vnd.in3d.3dml,3ds:image/x-3ds,3g2:video/3gpp2,3gp:video/3gpp,7z:application/x-7z-compressed,aab:application/x-authorware-bin,aac:audio/x-aac,aam:application/x-authorware-map,aas:application/x-authorware-seg,abw:application/x-abiword,ac:application/pkix-attr-cert,acc:application/vnd.americandynamics.acc,ace:application/x-ace-compressed,acu:application/vnd.acucobol,acutc:application/vnd.acucorp,adp:audio/adpcm,aep:application/vnd.audiograph,afm:application/x-font-type1,afp:application/vnd.ibm.modcap,ahead:application/vnd.ahead.space,ai:application/postscript,aif:audio/x-aiff,aifc:audio/x-aiff,aiff:audio/x-aiff,air:application/vnd.adobe.air-application-installer-package+zip,ait:application/vnd.dvb.ait,ami:application/vnd.amiga.ami,apk:application/vnd.android.package-archive,appcache:text/cache-manifest,application:application/x-ms-application,apr:application/vnd.lotus-approach,arc:application/x-freearc,asc:application/pgp-signature,asf:video/x-ms-asf,asm:text/x-asm,aso:application/vnd.accpac.simply.aso,asx:video/x-ms-asf,atc:application/vnd.acucorp,atom:application/atom+xml,atomcat:application/atomcat+xml,atomsvc:application/atomsvc+xml,atx:application/vnd.antix.game-component,au:audio/basic,avi:video/x-msvideo,aw:application/applixware,azf:application/vnd.airzip.filesecure.azf,azs:application/vnd.airzip.filesecure.azs,azw:application/vnd.amazon.ebook,bat:application/x-msdownload,bcpio:application/x-bcpio,bdf:application/x-font-bdf,bdm:application/vnd.syncml.dm+wbxml,bed:application/vnd.realvnc.bed,bh2:application/vnd.fujitsu.oasysprs,bin:application/octet-stream,blb:application/x-blorb,blorb:application/x-blorb,bmi:application/vnd.bmi,bmp:image/bmp,book:application/vnd.framemaker,box:application/vnd.previewsystems.box,boz:application/x-bzip2,bpk:application/octet-stream,btif:image/prs.btif,bz:application/x-bzip,bz2:application/x-bzip2,c:text/x-c,c11amc:application/vnd.cluetrust.cartomobile-config,c11amz:application/vnd.cluetrust.cartomobile-config-pkg,c4d:application/vnd.clonk.c4group,c4f:application/vnd.clonk.c4group,c4g:application/vnd.clonk.c4group,c4p:application/vnd.clonk.c4group,c4u:application/vnd.clonk.c4group,cab:application/vnd.ms-cab-compressed,caf:audio/x-caf,cap:application/vnd.tcpdump.pcap,car:application/vnd.curl.car,cat:application/vnd.ms-pki.seccat,cb7:application/x-cbr,cba:application/x-cbr,cbr:application/x-cbr,cbt:application/x-cbr,cbz:application/x-cbr,cc:text/x-c,cct:application/x-director,ccxml:application/ccxml+xml,cdbcmsg:application/vnd.contact.cmsg,cdf:application/x-netcdf,cdkey:application/vnd.mediastation.cdkey,cdmia:application/cdmi-capability,cdmic:application/cdmi-container,cdmid:application/cdmi-domain,cdmio:application/cdmi-object,cdmiq:application/cdmi-queue,cdx:chemical/x-cdx,cdxml:application/vnd.chemdraw+xml,cdy:application/vnd.cinderella,cer:application/pkix-cert,cfs:application/x-cfs-compressed,cgm:image/cgm,chat:application/x-chat,chm:application/vnd.ms-htmlhelp,chrt:application/vnd.kde.kchart,cif:chemical/x-cif,cii:application/vnd.anser-web-certificate-issue-initiation,cil:application/vnd.ms-artgalry,cla:application/vnd.claymore,class:application/java-vm,clkk:application/vnd.crick.clicker.keyboard,clkp:application/vnd.crick.clicker.palette,clkt:application/vnd.crick.clicker.template,clkw:application/vnd.crick.clicker.wordbank,clkx:application/vnd.crick.clicker,clp:application/x-msclip,cmc:application/vnd.cosmocaller,cmdf:chemical/x-cmdf,cml:chemical/x-cml,cmp:application/vnd.yellowriver-custom-menu,cmx:image/x-cmx,cod:application/vnd.rim.cod,com:application/x-msdownload,conf:text/plain; charset=utf-8,cpio:application/x-cpio,cpp:text/x-c,cpt:application/mac-compactpro,crd:application/x-mscardfile,crl:application/pkix-crl,crt:application/x-x509-ca-cert,cryptonote:application/vnd.rig.cryptonote,csh:application/x-csh,csml:chemical/x-csml,csp:application/vnd.commonspace,css:text/css,cst:application/x-director,csv:text/csv,cu:application/cu-seeme,curl:text/vnd.curl,cww:application/prs.cww,cxt:application/x-director,cxx:text/x-c,dae:model/vnd.collada+xml,daf:application/vnd.mobius.daf,dart:application/vnd.dart,dataless:application/vnd.fdsn.seed,davmount:application/davmount+xml,dbk:application/docbook+xml,dcr:application/x-director,dcurl:text/vnd.curl.dcurl,dd2:application/vnd.oma.dd2+xml,ddd:application/vnd.fujixerox.ddd,deb:application/x-debian-package,def:text/plain; charset=utf-8,deploy:application/octet-stream,der:application/x-x509-ca-cert,dfac:application/vnd.dreamfactory,dgc:application/x-dgc-compressed,dic:text/x-c,dir:application/x-director,dis:application/vnd.mobius.dis,dist:application/octet-stream,distz:application/octet-stream,djv:image/vnd.djvu,djvu:image/vnd.djvu,dll:application/x-msdownload,dmg:application/x-apple-diskimage,dmp:application/vnd.tcpdump.pcap,dms:application/octet-stream,dna:application/vnd.dna,doc:application/msword,docm:application/vnd.ms-word.document.macroenabled.12,docx:application/vnd.openxmlformats-officedocument.wordprocessingml.document,dot:application/msword,dotm:application/vnd.ms-word.template.macroenabled.12,dotx:application/vnd.openxmlformats-officedocument.wordprocessingml.template,dp:application/vnd.osgi.dp,dpg:application/vnd.dpgraph,dra:audio/vnd.dra,dsc:text/prs.lines.tag,dssc:application/dssc+der,dtb:application/x-dtbook+xml,dtd:application/xml-dtd,dts:audio/vnd.dts,dtshd:audio/vnd.dts.hd,dump:application/octet-stream,dvb:video/vnd.dvb.file,dvi:application/x-dvi,dwf:model/vnd.dwf,dwg:image/vnd.dwg,dxf:image/vnd.dxf,dxp:application/vnd.spotfire.dxp,dxr:application/x-director,ecelp4800:audio/vnd.nuera.ecelp4800,ecelp7470:audio/vnd.nuera.ecelp7470,ecelp9600:audio/vnd.nuera.ecelp9600,ecma:application/ecmascript,edm:application/vnd.novadigm.edm,edx:application/vnd.novadigm.edx,efif:application/vnd.picsel,ei6:application/vnd.pg.osasli,elc:application/octet-stream,emf:application/x-msmetafile,eml:message/rfc822,emma:application/emma+xml,emz:application/x-msmetafile,eol:audio/vnd.digital-winds,eot:application/vnd.ms-fontobject,eps:application/postscript,epub:application/epub+zip,es3:application/vnd.eszigno3+xml,esa:application/vnd.osgi.subsystem,esf:application/vnd.epson.esf,et3:application/vnd.eszigno3+xml,etx:text/x-setext,eva:application/x-eva,evy:application/x-envoy,exe:application/x-msdownload,exi:application/exi,ext:application/vnd.novadigm.ext,ez:application/andrew-inset,ez2:application/vnd.ezpix-album,ez3:application/vnd.ezpix-package,f:text/x-fortran,f4v:video/x-f4v,f77:text/x-fortran,f90:text/x-fortran,fbs:image/vnd.fastbidsheet,fcdt:application/vnd.adobe.formscentral.fcdt,fcs:application/vnd.isac.fcs,fdf:application/vnd.fdf,fe_launch:application/vnd.denovo.fcselayout-link,fg5:application/vnd.fujitsu.oasysgp,fgd:application/x-director,fh:image/x-freehand,fh4:image/x-freehand,fh5:image/x-freehand,fh7:image/x-freehand,fhc:image/x-freehand,fig:application/x-xfig,flac:audio/x-flac,fli:video/x-fli,flo:application/vnd.micrografx.flo,flv:video/x-flv,flw:application/vnd.kde.kivio,flx:text/vnd.fmi.flexstor,fly:text/vnd.fly,fm:application/vnd.framemaker,fnc:application/vnd.frogans.fnc,for:text/x-fortran,fpx:image/vnd.fpx,frame:application/vnd.framemaker,fsc:application/vnd.fsc.weblaunch,fst:image/vnd.fst,ftc:application/vnd.fluxtime.clip,fti:application/vnd.anser-web-funds-transfer-initiation,fvt:video/vnd.fvt,fxp:application/vnd.adobe.fxp,fxpl:application/vnd.adobe.fxp,fzs:application/vnd.fuzzysheet,g2w:application/vnd.geoplan,g3:image/g3fax,g3w:application/vnd.geospace,gac:application/vnd.groove-account,gam:application/x-tads,gbr:application/rpki-ghostbusters,gca:application/x-gca-compressed,gdl:model/vnd.gdl,geo:application/vnd.dynageo,gex:application/vnd.geometry-explorer,ggb:application/vnd.geogebra.file,ggt:application/vnd.geogebra.tool,ghf:application/vnd.groove-help,gif:image/gif,gim:application/vnd.groove-identity-message,gml:application/gml+xml,gmx:application/vnd.gmx,gnumeric:application/x-gnumeric,gph:application/vnd.flographit,gpx:application/gpx+xml,gqf:application/vnd.grafeq,gqs:application/vnd.grafeq,gram:application/srgs,gramps:application/x-gramps-xml,gre:application/vnd.geometry-explorer,grv:application/vnd.groove-injector,grxml:application/srgs+xml,gsf:application/x-font-ghostscript,gtar:application/x-gtar,gtm:application/vnd.groove-tool-message,gtw:model/vnd.gtw,gv:text/vnd.graphviz,gxf:application/gxf,gxt:application/vnd.geonext,gz:application/gzip,h:text/x-c,h261:video/h261,h263:video/h263,h264:video/h264,hal:application/vnd.hal+xml,hbci:application/vnd.hbci,hdf:application/x-hdf,hh:text/x-c,hlp:application/winhlp,hpgl:application/vnd.hp-hpgl,hpid:application/vnd.hp-hpid,hps:application/vnd.hp-hps,hqx:application/mac-binhex40,htke:application/vnd.kenameaapp,htm:text/html; charset=utf-8,html:text/html; charset=utf-8,hvd:application/vnd.yamaha.hv-dic,hvp:application/vnd.yamaha.hv-voice,hvs:application/vnd.yamaha.hv-script,i2g:application/vnd.intergeo,icc:application/vnd.iccprofile,ice:x-conference/x-cooltalk,icm:application/vnd.iccprofile,ico:image/x-icon,ics:text/calendar,ief:image/ief,ifb:text/calendar,ifm:application/vnd.shana.informed.formdata,iges:model/iges,igl:application/vnd.igloader,igm:application/vnd.insors.igm,igs:model/iges,igx:application/vnd.micrografx.igx,iif:application/vnd.shana.informed.interchange,imp:application/vnd.accpac.simply.imp,ims:application/vnd.ms-ims,in:text/plain; charset=utf-8,ink:application/inkml+xml,inkml:application/inkml+xml,install:application/x-install-instructions,iota:application/vnd.astraea-software.iota,ipfix:application/ipfix,ipk:application/vnd.shana.informed.package,irm:application/vnd.ibm.rights-management,irp:application/vnd.irepository.package+xml,iso:application/x-iso9660-image,itp:application/vnd.shana.informed.formtemplate,ivp:application/vnd.immervision-ivp,ivu:application/vnd.immervision-ivu,jad:text/vnd.sun.j2me.app-descriptor,jam:application/vnd.jam,jar:application/java-archive,java:text/x-java-source,jisp:application/vnd.jisp,jlt:application/vnd.hp-jlyt,jnlp:application/x-java-jnlp-file,joda:application/vnd.joost.joda-archive,jpe:image/jpeg,jpeg:image/jpeg,jpg:image/jpeg,jpgm:video/jpm,jpgv:video/jpeg,jpm:video/jpm,js:application/javascript; charset=utf-8,json:application/json,jsonml:application/jsonml+json,kar:audio/midi,karbon:application/vnd.kde.karbon,kfo:application/vnd.kde.kformula,kia:application/vnd.kidspiration,kml:application/vnd.google-earth.kml+xml,kmz:application/vnd.google-earth.kmz,kne:application/vnd.kinar,knp:application/vnd.kinar,kon:application/vnd.kde.kontour,kpr:application/vnd.kde.kpresenter,kpt:application/vnd.kde.kpresenter,kpxx:application/vnd.ds-keypoint,ksp:application/vnd.kde.kspread,ktr:application/vnd.kahootz,ktx:image/ktx,ktz:application/vnd.kahootz,kwd:application/vnd.kde.kword,kwt:application/vnd.kde.kword,lasxml:application/vnd.las.las+xml,latex:application/x-latex,lbd:application/vnd.llamagraphics.life-balance.desktop,lbe:application/vnd.llamagraphics.life-balance.exchange+xml,les:application/vnd.hhe.lesson-player,lha:application/x-lzh-compressed,link66:application/vnd.route66.link66+xml,list:text/plain; charset=utf-8,list3820:application/vnd.ibm.modcap,listafp:application/vnd.ibm.modcap,lnk:application/x-ms-shortcut,log:text/plain; charset=utf-8,lostxml:application/lost+xml,lrf:application/octet-stream,lrm:application/vnd.ms-lrm,ltf:application/vnd.frogans.ltf,lvp:audio/vnd.lucent.voice,lwp:application/vnd.lotus-wordpro,lzh:application/x-lzh-compressed,m13:application/x-msmediaview,m14:application/x-msmediaview,m1v:video/mpeg,m21:application/mp21,m2a:audio/mpeg,m2v:video/mpeg,m3a:audio/mpeg,m3u:audio/x-mpegurl,m3u8:application/vnd.apple.mpegurl,m4u:video/vnd.mpegurl,m4v:video/x-m4v,ma:application/mathematica,mads:application/mads+xml,mag:application/vnd.ecowin.chart,maker:application/vnd.framemaker,man:text/troff,mar:application/octet-stream,mathml:application/mathml+xml,mb:application/mathematica,mbk:application/vnd.mobius.mbk,mbox:application/mbox,mc1:application/vnd.medcalcdata,mcd:application/vnd.mcd,mcurl:text/vnd.curl.mcurl,mdb:application/x-msaccess,mdi:image/vnd.ms-modi,me:text/troff,mesh:model/mesh,meta4:application/metalink4+xml,metalink:application/metalink+xml,mets:application/mets+xml,mfm:application/vnd.mfmp,mft:application/rpki-manifest,mgp:application/vnd.osgeo.mapguide.package,mgz:application/vnd.proteus.magazine,mid:audio/midi,midi:audio/midi,mie:application/x-mie,mif:application/vnd.mif,mime:message/rfc822,mj2:video/mj2,mjp2:video/mj2,mjs:application/javascript; charset=utf-8,mk3d:video/x-matroska,mka:audio/x-matroska,mks:video/x-matroska,mkv:video/x-matroska,mlp:application/vnd.dolby.mlp,mmd:application/vnd.chipnuts.karaoke-mmd,mmf:application/vnd.smaf,mmr:image/vnd.fujixerox.edmics-mmr,mng:video/x-mng,mny:application/x-msmoney,mobi:application/x-mobipocket-ebook,mods:application/mods+xml,mov:video/quicktime,movie:video/x-sgi-movie,mp2:audio/mpeg,mp21:application/mp21,mp2a:audio/mpeg,mp3:audio/mpeg,mp4:video/mp4,mp4a:audio/mp4,mp4s:application/mp4,mp4v:video/mp4,mpc:application/vnd.mophun.certificate,mpe:video/mpeg,mpeg:video/mpeg,mpg:video/mpeg,mpg4:video/mp4,mpga:audio/mpeg,mpkg:application/vnd.apple.installer+xml,mpm:application/vnd.blueice.multipass,mpn:application/vnd.mophun.application,mpp:application/vnd.ms-project,mpt:application/vnd.ms-project,mpy:application/vnd.ibm.minipay,mqy:application/vnd.mobius.mqy,mrc:application/marc,mrcx:application/marcxml+xml,ms:text/troff,mscml:application/mediaservercontrol+xml,mseed:application/vnd.fdsn.mseed,mseq:application/vnd.mseq,msf:application/vnd.epson.msf,msh:model/mesh,msi:application/x-msdownload,msl:application/vnd.mobius.msl,msty:application/vnd.muvee.style,mts:model/vnd.mts,mus:application/vnd.musician,musicxml:application/vnd.recordare.musicxml+xml,mvb:application/x-msmediaview,mwf:application/vnd.mfer,mxf:application/mxf,mxl:application/vnd.recordare.musicxml,mxml:application/xv+xml,mxs:application/vnd.triscape.mxs,mxu:video/vnd.mpegurl,n-gage:application/vnd.nokia.n-gage.symbian.install,n3:text/n3,nb:application/mathematica,nbp:application/vnd.wolfram.player,nc:application/x-netcdf,ncx:application/x-dtbncx+xml,nfo:text/x-nfo,ngdat:application/vnd.nokia.n-gage.data,nitf:application/vnd.nitf,nlu:application/vnd.neurolanguage.nlu,nml:application/vnd.enliven,nnd:application/vnd.noblenet-directory,nns:application/vnd.noblenet-sealer,nnw:application/vnd.noblenet-web,npx:image/vnd.net-fpx,nsc:application/x-conference,nsf:application/vnd.lotus-notes,ntf:application/vnd.nitf,nzb:application/x-nzb,oa2:application/vnd.fujitsu.oasys2,oa3:application/vnd.fujitsu.oasys3,oas:application/vnd.fujitsu.oasys,obd:application/x-msbinder,obj:application/x-tgif,oda:application/oda,odb:application/vnd.oasis.opendocument.database,odc:application/vnd.oasis.opendocument.chart,odf:application/vnd.oasis.opendocument.formula,odft:application/vnd.oasis.opendocument.formula-template,odg:application/vnd.oasis.opendocument.graphics,odi:application/vnd.oasis.opendocument.image,odm:application/vnd.oasis.opendocument.text-master,odp:application/vnd.oasis.opendocument.presentation,ods:application/vnd.oasis.opendocument.spreadsheet,odt:application/vnd.oasis.opendocument.text,oga:audio/ogg,ogg:audio/ogg,ogv:video/ogg,ogx:application/ogg,omdoc:application/omdoc+xml,onepkg:application/onenote,onetmp:application/onenote,onetoc:application/onenote,onetoc2:application/onenote,opf:application/oebps-package+xml,opml:text/x-opml,oprc:application/vnd.palm,org:application/vnd.lotus-organizer,osf:application/vnd.yamaha.openscoreformat,osfpvg:application/vnd.yamaha.openscoreformat.osfpvg+xml,otc:application/vnd.oasis.opendocument.chart-template,otf:application/x-font-otf,otg:application/vnd.oasis.opendocument.graphics-template,oth:application/vnd.oasis.opendocument.text-web,oti:application/vnd.oasis.opendocument.image-template,otp:application/vnd.oasis.opendocument.presentation-template,ots:application/vnd.oasis.opendocument.spreadsheet-template,ott:application/vnd.oasis.opendocument.text-template,oxps:application/oxps,oxt:application/vnd.openofficeorg.extension,p:text/x-pascal,p10:application/pkcs10,p12:application/x-pkcs12,p7b:application/x-pkcs7-certificates,p7c:application/pkcs7-mime,p7m:application/pkcs7-mime,p7r:application/x-pkcs7-certreqresp,p7s:application/pkcs7-signature,p8:application/pkcs8,pas:text/x-pascal,paw:application/vnd.pawaafile,pbd:application/vnd.powerbuilder6,pbm:image/x-portable-bitmap,pcap:application/vnd.tcpdump.pcap,pcf:application/x-font-pcf,pcl:application/vnd.hp-pcl,pclxl:application/vnd.hp-pclxl,pct:image/x-pict,pcurl:application/vnd.curl.pcurl,pcx:image/x-pcx,pdb:application/vnd.palm,pdf:application/pdf,pfa:application/x-font-type1,pfb:application/x-font-type1,pfm:application/x-font-type1,pfr:application/font-tdpfr,pfx:application/x-pkcs12,pgm:image/x-portable-graymap,pgn:application/x-chess-pgn,pgp:application/pgp-encrypted,pic:image/x-pict,pkg:application/octet-stream,pki:application/pkixcmp,pkipath:application/pkix-pkipath,plb:application/vnd.3gpp.pic-bw-large,plc:application/vnd.mobius.plc,plf:application/vnd.pocketlearn,pls:application/pls+xml,pmd:application/x-pmd,pml:application/vnd.ctc-posml,png:image/png,pnm:image/x-portable-anymap,portpkg:application/vnd.macports.portpkg,pot:application/vnd.ms-powerpoint,potm:application/vnd.ms-powerpoint.template.macroenabled.12,potx:application/vnd.openxmlformats-officedocument.presentationml.template,ppam:application/vnd.ms-powerpoint.addin.macroenabled.12,ppd:application/vnd.cups-ppd,ppm:image/x-portable-pixmap,pps:application/vnd.ms-powerpoint,ppsm:application/vnd.ms-powerpoint.slideshow.macroenabled.12,ppsx:application/vnd.openxmlformats-officedocument.presentationml.slideshow,ppt:application/vnd.ms-powerpoint,pptm:application/vnd.ms-powerpoint.presentation.macroenabled.12,pptx:application/vnd.openxmlformats-officedocument.presentationml.presentation,pqa:application/vnd.palm,prc:application/x-mobipocket-ebook,pre:application/vnd.lotus-freelance,prf:application/pics-rules,ps:application/postscript,psb:application/vnd.3gpp.pic-bw-small,psd:image/vnd.adobe.photoshop,psf:application/x-font-linux-psf,pskcxml:application/pskc+xml,ptid:application/vnd.pvi.ptid1,pub:application/x-mspublisher,pvb:application/vnd.3gpp.pic-bw-var,pwn:application/vnd.3m.post-it-notes,pya:audio/vnd.ms-playready.media.pya,pyv:video/vnd.ms-playready.media.pyv,qam:application/vnd.epson.quickanime,qbo:application/vnd.intu.qbo,qfx:application/vnd.intu.qfx,qps:application/vnd.publishare-delta-tree,qt:video/quicktime,qwd:application/vnd.quark.quarkxpress,qwt:application/vnd.quark.quarkxpress,qxb:application/vnd.quark.quarkxpress,qxd:application/vnd.quark.quarkxpress,qxl:application/vnd.quark.quarkxpress,qxt:application/vnd.quark.quarkxpress,ra:audio/x-pn-realaudio,ram:audio/x-pn-realaudio,rar:application/x-rar-compressed,ras:image/x-cmu-raster,rcprofile:application/vnd.ipunplugged.rcprofile,rdf:application/rdf+xml,rdz:application/vnd.data-vision.rdz,rep:application/vnd.businessobjects,res:application/x-dtbresource+xml,rgb:image/x-rgb,rif:application/reginfo+xml,rip:audio/vnd.rip,ris:application/x-research-info-systems,rl:application/resource-lists+xml,rlc:image/vnd.fujixerox.edmics-rlc,rld:application/resource-lists-diff+xml,rm:application/vnd.rn-realmedia,rmi:audio/midi,rmp:audio/x-pn-realaudio-plugin,rms:application/vnd.jcp.javame.midlet-rms,rmvb:application/vnd.rn-realmedia-vbr,rnc:application/relax-ng-compact-syntax,roa:application/rpki-roa,roff:text/troff,rp9:application/vnd.cloanto.rp9,rpss:application/vnd.nokia.radio-presets,rpst:application/vnd.nokia.radio-preset,rq:application/sparql-query,rs:application/rls-services+xml,rsd:application/rsd+xml,rss:application/rss+xml; charset=utf-8,rtf:application/rtf,rtx:text/richtext,s:text/x-asm,s3m:audio/s3m,saf:application/vnd.yamaha.smaf-audio,sbml:application/sbml+xml,sc:application/vnd.ibm.secure-container,scd:application/x-msschedule,scm:application/vnd.lotus-screencam,scq:application/scvp-cv-request,scs:application/scvp-cv-response,scurl:text/vnd.curl.scurl,sda:application/vnd.stardivision.draw,sdc:application/vnd.stardivision.calc,sdd:application/vnd.stardivision.impress,sdkd:application/vnd.solent.sdkm+xml,sdkm:application/vnd.solent.sdkm+xml,sdp:application/sdp,sdw:application/vnd.stardivision.writer,see:application/vnd.seemail,seed:application/vnd.fdsn.seed,sema:application/vnd.sema,semd:application/vnd.semd,semf:application/vnd.semf,ser:application/java-serialized-object,setpay:application/set-payment-initiation,setreg:application/set-registration-initiation,sfd-hdstx:application/vnd.hydrostatix.sof-data,sfs:application/vnd.spotfire.sfs,sfv:text/x-sfv,sgi:image/sgi,sgl:application/vnd.stardivision.writer-global,sgm:text/sgml,sgml:text/sgml,sh:application/x-sh,shar:application/x-shar,shf:application/shf+xml,sid:image/x-mrsid-image,sig:application/pgp-signature,sil:audio/silk,silo:model/mesh,sis:application/vnd.symbian.install,sisx:application/vnd.symbian.install,sit:application/x-stuffit,sitx:application/x-stuffitx,skd:application/vnd.koan,skm:application/vnd.koan,skp:application/vnd.koan,skt:application/vnd.koan,sldm:application/vnd.ms-powerpoint.slide.macroenabled.12,sldx:application/vnd.openxmlformats-officedocument.presentationml.slide,slt:application/vnd.epson.salt,sm:application/vnd.stepmania.stepchart,smf:application/vnd.stardivision.math,smi:application/smil+xml,smil:application/smil+xml,smv:video/x-smv,smzip:application/vnd.stepmania.package,snd:audio/basic,snf:application/x-font-snf,so:application/octet-stream,spc:application/x-pkcs7-certificates,spf:application/vnd.yamaha.smaf-phrase,spl:application/x-futuresplash,spot:text/vnd.in3d.spot,spp:application/scvp-vp-response,spq:application/scvp-vp-request,spx:audio/ogg,sql:application/x-sql,src:application/x-wais-source,srt:application/x-subrip,sru:application/sru+xml,srx:application/sparql-results+xml,ssdl:application/ssdl+xml,sse:application/vnd.kodak-descriptor,ssf:application/vnd.epson.ssf,ssml:application/ssml+xml,st:application/vnd.sailingtracker.track,stc:application/vnd.sun.xml.calc.template,std:application/vnd.sun.xml.draw.template,stf:application/vnd.wt.stf,sti:application/vnd.sun.xml.impress.template,stk:application/hyperstudio,stl:application/vnd.ms-pki.stl,str:application/vnd.pg.format,stw:application/vnd.sun.xml.writer.template,sub:text/vnd.dvb.subtitle,sus:application/vnd.sus-calendar,susp:application/vnd.sus-calendar,sv4cpio:application/x-sv4cpio,sv4crc:application/x-sv4crc,svc:application/vnd.dvb.service,svd:application/vnd.svd,svg:image/svg+xml,svgz:image/svg+xml,swa:application/x-director,swf:application/x-shockwave-flash,swi:application/vnd.aristanetworks.swi,sxc:application/vnd.sun.xml.calc,sxd:application/vnd.sun.xml.draw,sxg:application/vnd.sun.xml.writer.global,sxi:application/vnd.sun.xml.impress,sxm:application/vnd.sun.xml.math,sxw:application/vnd.sun.xml.writer,t:text/troff,t3:application/x-t3vm-image,taglet:application/vnd.mynfc,tao:application/vnd.tao.intent-module-archive,tar:application/x-tar,tcap:application/vnd.3gpp2.tcap,tcl:application/x-tcl,teacher:application/vnd.smart.teacher,tei:application/tei+xml,teicorpus:application/tei+xml,tex:application/x-tex,texi:application/x-texinfo,texinfo:application/x-texinfo,text:text/plain; charset=utf-8,tfi:application/thraud+xml,tfm:application/x-tex-tfm,tga:image/x-tga,thmx:application/vnd.ms-officetheme,tif:image/tiff,tiff:image/tiff,tmo:application/vnd.tmobile-livetv,torrent:application/x-bittorrent,tpl:application/vnd.groove-tool-template,tpt:application/vnd.trid.tpt,tr:text/troff,tra:application/vnd.trueapp,trm:application/x-msterminal,tsd:application/timestamped-data,tsv:text/tab-separated-values,ttc:application/x-font-ttf,ttf:application/x-font-ttf,ttl:text/turtle,twd:application/vnd.simtech-mindmapper,twds:application/vnd.simtech-mindmapper,txd:application/vnd.genomatix.tuxedo,txf:application/vnd.mobius.txf,txt:text/plain; charset=utf-8,u32:application/x-authorware-bin,udeb:application/x-debian-package,ufd:application/vnd.ufdl,ufdl:application/vnd.ufdl,ulx:application/x-glulx,umj:application/vnd.umajin,unityweb:application/vnd.unity,uoml:application/vnd.uoml+xml,uri:text/uri-list,uris:text/uri-list,urls:text/uri-list,ustar:application/x-ustar,utz:application/vnd.uiq.theme,uu:text/x-uuencode,uva:audio/vnd.dece.audio,uvd:application/vnd.dece.data,uvf:application/vnd.dece.data,uvg:image/vnd.dece.graphic,uvh:video/vnd.dece.hd,uvi:image/vnd.dece.graphic,uvm:video/vnd.dece.mobile,uvp:video/vnd.dece.pd,uvs:video/vnd.dece.sd,uvt:application/vnd.dece.ttml+xml,uvu:video/vnd.uvvu.mp4,uvv:video/vnd.dece.video,uvva:audio/vnd.dece.audio,uvvd:application/vnd.dece.data,uvvf:application/vnd.dece.data,uvvg:image/vnd.dece.graphic,uvvh:video/vnd.dece.hd,uvvi:image/vnd.dece.graphic,uvvm:video/vnd.dece.mobile,uvvp:video/vnd.dece.pd,uvvs:video/vnd.dece.sd,uvvt:application/vnd.dece.ttml+xml,uvvu:video/vnd.uvvu.mp4,uvvv:video/vnd.dece.video,uvvx:application/vnd.dece.unspecified,uvvz:application/vnd.dece.zip,uvx:application/vnd.dece.unspecified,uvz:application/vnd.dece.zip,vcard:text/vcard,vcd:application/x-cdlink,vcf:text/x-vcard,vcg:application/vnd.groove-vcard,vcs:text/x-vcalendar,vcx:application/vnd.vcx,vis:application/vnd.visionary,viv:video/vnd.vivo,vmd:application/vocaltec-media-desc,vob:video/x-ms-vob,vor:application/vnd.stardivision.writer,vox:application/x-authorware-bin,vrml:model/vrml,vsd:application/vnd.visio,vsf:application/vnd.vsf,vss:application/vnd.visio,vst:application/vnd.visio,vsw:application/vnd.visio,vtu:model/vnd.vtu,vtt:text/vtt,vxml:application/voicexml+xml,w3d:application/x-director,wad:application/x-doom,wasm:application/wasm,wav:audio/x-wav,wax:audio/x-ms-wax,wbmp:image/vnd.wap.wbmp,wbs:application/vnd.criticaltools.wbs+xml,wbxml:application/vnd.wap.wbxml,wcm:application/vnd.ms-works,wdb:application/vnd.ms-works,wdp:image/vnd.ms-photo,weba:audio/webm,webm:video/webm,webp:image/webp,wg:application/vnd.pmi.widget,wgt:application/widget,wks:application/vnd.ms-works,wm:video/x-ms-wm,wma:audio/x-ms-wma,wmd:application/x-ms-wmd,wmf:application/x-msmetafile,wml:text/vnd.wap.wml; charset=utf-8,wmlc:application/vnd.wap.wmlc,wmls:text/vnd.wap.wmlscript,wmlsc:application/vnd.wap.wmlscriptc,wmv:video/x-ms-wmv,wmx:video/x-ms-wmx,wmz:application/x-msmetafile,woff:application/x-font-woff,wpd:application/vnd.wordperfect,wpl:application/vnd.ms-wpl,wps:application/vnd.ms-works,wqd:application/vnd.wqd,wri:application/x-mswrite,wrl:model/vrml,wsdl:application/wsdl+xml,wspolicy:application/wspolicy+xml,wtb:application/vnd.webturbo,wvx:video/x-ms-wvx,x32:application/x-authorware-bin,x3d:model/x3d+xml,x3db:model/x3d+binary,x3dbz:model/x3d+binary,x3dv:model/x3d+vrml,x3dvz:model/x3d+vrml,x3dz:model/x3d+xml,xaml:application/xaml+xml,xap:application/x-silverlight-app,xar:application/vnd.xara,xbap:application/x-ms-xbap,xbd:application/vnd.fujixerox.docuworks.binder,xbm:image/x-xbitmap,xdf:application/xcap-diff+xml,xdm:application/vnd.syncml.dm+xml,xdp:application/vnd.adobe.xdp+xml,xdssc:application/dssc+xml,xdw:application/vnd.fujixerox.docuworks,xenc:application/xenc+xml,xer:application/patch-ops-error+xml,xfdf:application/vnd.adobe.xfdf,xfdl:application/vnd.xfdl,xht:application/xhtml+xml,xhtml:application/xhtml+xml,xhvml:application/xv+xml,xif:image/vnd.xiff,xla:application/vnd.ms-excel,xlam:application/vnd.ms-excel.addin.macroenabled.12,xlc:application/vnd.ms-excel,xlf:application/x-xliff+xml,xlm:application/vnd.ms-excel,xls:application/vnd.ms-excel,xlsb:application/vnd.ms-excel.sheet.binary.macroenabled.12,xlsm:application/vnd.ms-excel.sheet.macroenabled.12,xlsx:application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,xlt:application/vnd.ms-excel,xltm:application/vnd.ms-excel.template.macroenabled.12,xltx:application/vnd.openxmlformats-officedocument.spreadsheetml.template,xlw:application/vnd.ms-excel,xm:audio/xm,xml:application/xml,xo:application/vnd.olpc-sugar,xop:application/xop+xml,xpi:application/x-xpinstall,xpl:application/xproc+xml,xpm:image/x-xpixmap,xpr:application/vnd.is-xpr,xps:application/vnd.ms-xpsdocument,xpw:application/vnd.intercon.formnet,xpx:application/vnd.intercon.formnet,xsl:application/xml,xslt:application/xslt+xml,xsm:application/vnd.syncml+xml,xspf:application/xspf+xml,xul:application/vnd.mozilla.xul+xml,xvm:application/xv+xml,xvml:application/xv+xml,xwd:image/x-xwindowdump,xyz:chemical/x-xyz,xz:application/x-xz,yang:application/yang,yin:application/yin+xml,z1:application/x-zmachine,z2:application/x-zmachine,z3:application/x-zmachine,z4:application/x-zmachine,z5:application/x-zmachine,z6:application/x-zmachine,z7:application/x-zmachine,z8:application/x-zmachine,zaz:application/vnd.zzazz.deck+xml,zip:application/zip,zir:application/vnd.zul,zirz:application/vnd.zul,zmm:application/vnd.handheld-entertainment+xml";
    private const string directoryListingTemplate = "<!DOCTYPE html>\n\n<html dir=\"ltr\" lang=\"en\">\n\n<head>\n<meta charset=\"utf-8\">\n<meta name=\"google\" value=\"notranslate\">\n\n<script>\nfunction addRow(name, url, isdir,\n    size, size_string, date_modified, date_modified_string) {\n  if (name == \".\" || name == \"..\")\n    return;\n\n  var root = document.location.pathname;\n  if (root.substr(-1) !== \"/\")\n    root += \"/\";\n\n  var tbody = document.getElementById(\"tbody\");\n  var row = document.createElement(\"tr\");\n  var file_cell = document.createElement(\"td\");\n  var link = document.createElement(\"a\");\n\n  link.className = isdir ? \"icon dir\" : \"icon file\";\n\n  if (isdir) {\n    name = name + \"/\";\n    url = url + \"/\";\n    size = 0;\n    size_string = \"\";\n  } else {\n    link.draggable = \"true\";\n    link.addEventListener(\"dragstart\", onDragStart, false);\n  }\n  link.innerText = name;\n  link.href = root + url;\n\n  file_cell.dataset.value = name;\n  file_cell.appendChild(link);\n\n  row.appendChild(file_cell);\n  row.appendChild(createCell(size, size_string));\n  row.appendChild(createCell(date_modified, date_modified_string));\n\n  tbody.appendChild(row);\n}\n\nfunction onDragStart(e) {\n  var el = e.srcElement;\n  var name = el.innerText.replace(\":\", \"\");\n  var download_url_data = \"application/octet-stream:\" + name + \":\" + el.href;\n  e.dataTransfer.setData(\"DownloadURL\", download_url_data);\n  e.dataTransfer.effectAllowed = \"copy\";\n}\n\nfunction createCell(value, text) {\n  var cell = document.createElement(\"td\");\n  cell.setAttribute(\"class\", \"detailsColumn\");\n  cell.dataset.value = value;\n  cell.innerText = text;\n  return cell;\n}\n\nfunction start(location) {\n  var header = document.getElementById(\"header\");\n  header.innerText = header.innerText.replace(\"LOCATION\", location);\n\n  document.getElementById(\"title\").innerText = header.innerText;\n}\n\nfunction onHasParentDirectory() {\n  var box = document.getElementById(\"parentDirLinkBox\");\n  box.style.display = \"block\";\n\n  var root = document.location.pathname;\n  if (!root.endsWith(\"/\"))\n    root += \"/\";\n\n  var link = document.getElementById(\"parentDirLink\");\n  link.href = root + \"..\";\n}\n\nfunction onListingParsingError() {\n  var box = document.getElementById(\"listingParsingErrorBox\");\n  box.innerHTML = box.innerHTML.replace(\"LOCATION\", encodeURI(document.location)\n      + \"?raw\");\n  box.style.display = \"block\";\n}\n\nfunction sortTable(column) {\n  var theader = document.getElementById(\"theader\");\n  var oldOrder = theader.cells[column].dataset.order || '1';\n  oldOrder = parseInt(oldOrder, 10)\n  var newOrder = 0 - oldOrder;\n  theader.cells[column].dataset.order = newOrder;\n\n  var tbody = document.getElementById(\"tbody\");\n  var rows = tbody.rows;\n  var list = [], i;\n  for (i = 0; i < rows.length; i++) {\n    list.push(rows[i]);\n  }\n\n  list.sort(function(row1, row2) {\n    var a = row1.cells[column].dataset.value;\n    var b = row2.cells[column].dataset.value;\n    if (column) {\n      a = parseInt(a, 10);\n      b = parseInt(b, 10);\n      return a > b ? newOrder : a < b ? oldOrder : 0;\n    }\n\n    // Column 0 is text.\n    if (a > b)\n      return newOrder;\n    if (a < b)\n      return oldOrder;\n    return 0;\n  });\n\n  // Appending an existing child again just moves it.\n  for (i = 0; i < list.length; i++) {\n    tbody.appendChild(list[i]);\n  }\n}\n\n// Add event handlers to column headers.\nfunction addHandlers(element, column) {\n  element.onclick = (e) => sortTable(column);\n  element.onkeydown = (e) => {\n    if (e.key == 'Enter' || e.key == ' ') {\n      sortTable(column);\n      e.preventDefault();\n    }\n  };\n}\n\nfunction onLoad() {\n  addHandlers(document.getElementById('nameColumnHeader'), 0);\n  addHandlers(document.getElementById('sizeColumnHeader'), 1);\n  addHandlers(document.getElementById('dateColumnHeader'), 2);\n}\n\nwindow.addEventListener('DOMContentLoaded', onLoad);\n</script>\n\n<style>\n\n  h1 {\n    border-bottom: 1px solid #c0c0c0;\n    margin-bottom: 10px;\n    padding-bottom: 10px;\n    white-space: nowrap;\n  }\n\n  table {\n    border-collapse: collapse;\n  }\n\n  th {\n    cursor: pointer;\n  }\n\n  td.detailsColumn {\n    -webkit-padding-start: 2em;\n    text-align: end;\n    white-space: nowrap;\n  }\n\n  a.icon {\n    -webkit-padding-start: 1.5em;\n    text-decoration: none;\n    user-select: auto;\n  }\n\n  a.icon:hover {\n    text-decoration: underline;\n  }\n\n  a.file {\n    background : url(\"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAIAAACQkWg2AAAABnRSTlMAAAAAAABupgeRAAABHUlEQVR42o2RMW7DIBiF3498iHRJD5JKHurL+CRVBp+i2T16tTynF2gO0KSb5ZrBBl4HHDBuK/WXACH4eO9/CAAAbdvijzLGNE1TVZXfZuHg6XCAQESAZXbOKaXO57eiKG6ft9PrKQIkCQqFoIiQFBGlFIB5nvM8t9aOX2Nd18oDzjnPgCDpn/BH4zh2XZdlWVmWiUK4IgCBoFMUz9eP6zRN75cLgEQhcmTQIbl72O0f9865qLAAsURAAgKBJKEtgLXWvyjLuFsThCSstb8rBCaAQhDYWgIZ7myM+TUBjDHrHlZcbMYYk34cN0YSLcgS+wL0fe9TXDMbY33fR2AYBvyQ8L0Gk8MwREBrTfKe4TpTzwhArXWi8HI84h/1DfwI5mhxJamFAAAAAElFTkSuQmCC \") left top no-repeat;\n  }\n\n  a.dir {\n    background : url(\"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAd5JREFUeNqMU79rFUEQ/vbuodFEEkzAImBpkUabFP4ldpaJhZXYm/RiZWsv/hkWFglBUyTIgyAIIfgIRjHv3r39MePM7N3LcbxAFvZ2b2bn22/mm3XMjF+HL3YW7q28YSIw8mBKoBihhhgCsoORot9d3/ywg3YowMXwNde/PzGnk2vn6PitrT+/PGeNaecg4+qNY3D43vy16A5wDDd4Aqg/ngmrjl/GoN0U5V1QquHQG3q+TPDVhVwyBffcmQGJmSVfyZk7R3SngI4JKfwDJ2+05zIg8gbiereTZRHhJ5KCMOwDFLjhoBTn2g0ghagfKeIYJDPFyibJVBtTREwq60SpYvh5++PpwatHsxSm9QRLSQpEVSd7/TYJUb49TX7gztpjjEffnoVw66+Ytovs14Yp7HaKmUXeX9rKUoMoLNW3srqI5fWn8JejrVkK0QcrkFLOgS39yoKUQe292WJ1guUHG8K2o8K00oO1BTvXoW4yasclUTgZYJY9aFNfAThX5CZRmczAV52oAPoupHhWRIUUAOoyUIlYVaAa/VbLbyiZUiyFbjQFNwiZQSGl4IDy9sO5Wrty0QLKhdZPxmgGcDo8ejn+c/6eiK9poz15Kw7Dr/vN/z6W7q++091/AQYA5mZ8GYJ9K0AAAAAASUVORK5CYII= \") left top no-repeat;\n  }\n\n  a.up {\n    background : url(\"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAmlJREFUeNpsU0toU0EUPfPysx/tTxuDH9SCWhUDooIbd7oRUUTMouqi2iIoCO6lceHWhegy4EJFinWjrlQUpVm0IIoFpVDEIthm0dpikpf3ZuZ6Z94nrXhhMjM3c8895977BBHB2PznK8WPtDgyWH5q77cPH8PpdXuhpQT4ifR9u5sfJb1bmw6VivahATDrxcRZ2njfoaMv+2j7mLDn93MPiNRMvGbL18L9IpF8h9/TN+EYkMffSiOXJ5+hkD+PdqcLpICWHOHc2CC+LEyA/K+cKQMnlQHJX8wqYG3MAJy88Wa4OLDvEqAEOpJd0LxHIMdHBziowSwVlF8D6QaicK01krw/JynwcKoEwZczewroTvZirlKJs5CqQ5CG8pb57FnJUA0LYCXMX5fibd+p8LWDDemcPZbzQyjvH+Ki1TlIciElA7ghwLKV4kRZstt2sANWRjYTAGzuP2hXZFpJ/GsxgGJ0ox1aoFWsDXyyxqCs26+ydmagFN/rRjymJ1898bzGzmQE0HCZpmk5A0RFIv8Pn0WYPsiu6t/Rsj6PauVTwffTSzGAGZhUG2F06hEc9ibS7OPMNp6ErYFlKavo7MkhmTqCxZ/jwzGA9Hx82H2BZSw1NTN9Gx8ycHkajU/7M+jInsDC7DiaEmo1bNl1AMr9ASFgqVu9MCTIzoGUimXVAnnaN0PdBBDCCYbEtMk6wkpQwIG0sn0PQIUF4GsTwLSIFKNqF6DVrQq+IWVrQDxAYQC/1SsYOI4pOxKZrfifiUSbDUisif7XlpGIPufXd/uvdvZm760M0no1FZcnrzUdjw7au3vu/BVgAFLXeuTxhTXVAAAAAElFTkSuQmCC \") left top no-repeat;\n  }\n\n  html[dir=rtl] a {\n    background-position-x: right;\n  }\n\n  #parentDirLinkBox {\n    margin-bottom: 10px;\n    padding-bottom: 10px;\n  }\n\n  #listingParsingErrorBox {\n    border: 1px solid black;\n    background: #fae691;\n    padding: 10px;\n    display: none;\n  }\n</style>\n\n<title id=\"title\"></title>\n\n</head>\n\n<body>\n\n<div id=\"listingParsingErrorBox\">Oh, no! This server is sending data Google Chrome can't understand. Please <a href=\"http://code.google.com/p/chromium/issues/entry\">report a bug</a>, and include the <a href=\"LOCATION\">raw listing</a>.</div>\n\n<h1 id=\"header\">Index of LOCATION</h1>\n\n<div id=\"parentDirLinkBox\" style=\"display:none\">\n  <a id=\"parentDirLink\" class=\"icon up\">\n    <span id=\"parentDirText\">[parent directory]</span>\n  </a>\n</div>\n\n<table>\n  <thead>\n    <tr class=\"header\" id=\"theader\">\n      <th id=\"nameColumnHeader\" tabindex=0 role=\"button\">Name</th>\n      <th id=\"sizeColumnHeader\" class=\"detailsColumn\" tabindex=0 role=\"button\">\n        Size\n      </th>\n      <th id=\"dateColumnHeader\" class=\"detailsColumn\" tabindex=0 role=\"button\">\n        Date Modified\n      </th>\n    </tr>\n  </thead>\n  <tbody id=\"tbody\">\n  </tbody>\n</table>\n\n</body>\n\n</html>\n<script>// Copyright (c) 2012 The Chromium Authors. All rights reserved.\n// Use of this source code is governed by a BSD-style license that can be\n// found in the LICENSE file.\n\n/**\n * @fileoverview This file defines a singleton which provides access to all data\n * that is available as soon as the page's resources are loaded (before DOM\n * content has finished loading). This data includes both localized strings and\n * any data that is important to have ready from a very early stage (e.g. things\n * that must be displayed right away).\n *\n * Note that loadTimeData is not guaranteed to be consistent between page\n * refreshes (https://crbug.com/740629) and should not contain values that might\n * change if the page is re-opened later.\n */\n\n// #import {assert} from './assert.m.js';\n// #import {parseHtmlSubset} from './parse_html_subset.m.js';\n\n/**\n * @typedef {{\n *   substitutions: (!Array<string>|undefined),\n *   attrs: (!Array<string>|undefined),\n *   tags: (!Array<string>|undefined),\n * }}\n */\n/* #export */ let SanitizeInnerHtmlOpts;\n\n// eslint-disable-next-line no-var\n/* #export */ /** @type {!LoadTimeData} */ var loadTimeData;\n\nclass LoadTimeData {\n  constructor() {\n    /** @type {Object} */\n    this.data_;\n  }\n\n  /**\n   * Sets the backing object.\n   *\n   * Note that there is no getter for |data_| to discourage abuse of the form:\n   *\n   *     var value = loadTimeData.data()['key'];\n   *\n   * @param {Object} value The de-serialized page data.\n   */\n  set data(value) {\n    expect(!this.data_, 'Re-setting data.');\n    this.data_ = value;\n  }\n\n  /**\n   * Returns a JsEvalContext for |data_|.\n   * @returns {JsEvalContext}\n   */\n  createJsEvalContext() {\n    return new JsEvalContext(this.data_);\n  }\n\n  /**\n   * @param {string} id An ID of a value that might exist.\n   * @return {boolean} True if |id| is a key in the dictionary.\n   */\n  valueExists(id) {\n    return id in this.data_;\n  }\n\n  /**\n   * Fetches a value, expecting that it exists.\n   * @param {string} id The key that identifies the desired value.\n   * @return {*} The corresponding value.\n   */\n  getValue(id) {\n    expect(this.data_, 'No data. Did you remember to include strings.js?');\n    const value = this.data_[id];\n    expect(typeof value !== 'undefined', 'Could not find value for ' + id);\n    return value;\n  }\n\n  /**\n   * As above, but also makes sure that the value is a string.\n   * @param {string} id The key that identifies the desired string.\n   * @return {string} The corresponding string value.\n   */\n  getString(id) {\n    const value = this.getValue(id);\n    expectIsType(id, value, 'string');\n    return /** @type {string} */ (value);\n  }\n\n  /**\n   * Returns a formatted localized string where $1 to $9 are replaced by the\n   * second to the tenth argument.\n   * @param {string} id The ID of the string we want.\n   * @param {...(string|number)} var_args The extra values to include in the\n   *     formatted output.\n   * @return {string} The formatted string.\n   */\n  getStringF(id, var_args) {\n    const value = this.getString(id);\n    if (!value) {\n      return '';\n    }\n\n    const args = Array.prototype.slice.call(arguments);\n    args[0] = value;\n    return this.substituteString.apply(this, args);\n  }\n\n  /**\n   * Make a string safe for use with with Polymer bindings that are\n   * inner-h-t-m-l (or other innerHTML use).\n   * @param {string} rawString The unsanitized string.\n   * @param {SanitizeInnerHtmlOpts=} opts Optional additional allowed tags and\n   *     attributes.\n   * @return {string}\n   */\n  sanitizeInnerHtml(rawString, opts) {\n    opts = opts || {};\n    return parseHtmlSubset('<b>' + rawString + '</b>', opts.tags, opts.attrs)\n        .firstChild.innerHTML;\n  }\n\n  /**\n   * Returns a formatted localized string where $1 to $9 are replaced by the\n   * second to the tenth argument. Any standalone $ signs must be escaped as\n   * $$.\n   * @param {string} label The label to substitute through.\n   *     This is not an resource ID.\n   * @param {...(string|number)} var_args The extra values to include in the\n   *     formatted output.\n   * @return {string} The formatted string.\n   */\n  substituteString(label, var_args) {\n    const varArgs = arguments;\n    return label.replace(/\\$(.|$|\\n)/g, function(m) {\n      assert(m.match(/\\$[$1-9]/), 'Unescaped $ found in localized string.');\n      return m === '$$' ? '$' : varArgs[m[1]];\n    });\n  }\n\n  /**\n   * Returns a formatted string where $1 to $9 are replaced by the second to\n   * tenth argument, split apart into a list of pieces describing how the\n   * substitution was performed. Any standalone $ signs must be escaped as $$.\n   * @param {string} label A localized string to substitute through.\n   *     This is not an resource ID.\n   * @param {...(string|number)} var_args The extra values to include in the\n   *     formatted output.\n   * @return {!Array<!{value: string, arg: (null|string)}>} The formatted\n   *     string pieces.\n   */\n  getSubstitutedStringPieces(label, var_args) {\n    const varArgs = arguments;\n    // Split the string by separately matching all occurrences of $1-9 and of\n    // non $1-9 pieces.\n    const pieces = (label.match(/(\\$[1-9])|(([^$]|\\$([^1-9]|$))+)/g) ||\n                    []).map(function(p) {\n      // Pieces that are not $1-9 should be returned after replacing $$\n      // with $.\n      if (!p.match(/^\\$[1-9]$/)) {\n        assert(\n            (p.match(/\\$/g) || []).length % 2 === 0,\n            'Unescaped $ found in localized string.');\n        return {value: p.replace(/\\$\\$/g, '$'), arg: null};\n      }\n\n      // Otherwise, return the substitution value.\n      return {value: varArgs[p[1]], arg: p};\n    });\n\n    return pieces;\n  }\n\n  /**\n   * As above, but also makes sure that the value is a boolean.\n   * @param {string} id The key that identifies the desired boolean.\n   * @return {boolean} The corresponding boolean value.\n   */\n  getBoolean(id) {\n    const value = this.getValue(id);\n    expectIsType(id, value, 'boolean');\n    return /** @type {boolean} */ (value);\n  }\n\n  /**\n   * As above, but also makes sure that the value is an integer.\n   * @param {string} id The key that identifies the desired number.\n   * @return {number} The corresponding number value.\n   */\n  getInteger(id) {\n    const value = this.getValue(id);\n    expectIsType(id, value, 'number');\n    expect(value === Math.floor(value), 'Number isn\\'t integer: ' + value);\n    return /** @type {number} */ (value);\n  }\n\n  /**\n   * Override values in loadTimeData with the values found in |replacements|.\n   * @param {Object} replacements The dictionary object of keys to replace.\n   */\n  overrideValues(replacements) {\n    expect(\n        typeof replacements === 'object',\n        'Replacements must be a dictionary object.');\n    for (const key in replacements) {\n      this.data_[key] = replacements[key];\n    }\n  }\n}\n\n  /**\n   * Checks condition, displays error message if expectation fails.\n   * @param {*} condition The condition to check for truthiness.\n   * @param {string} message The message to display if the check fails.\n   */\n  function expect(condition, message) {\n    if (!condition) {\n      console.error(\n          'Unexpected condition on ' + document.location.href + ': ' + message);\n    }\n  }\n\n  /**\n   * Checks that the given value has the given type.\n   * @param {string} id The id of the value (only used for error message).\n   * @param {*} value The value to check the type on.\n   * @param {string} type The type we expect |value| to be.\n   */\n  function expectIsType(id, value, type) {\n    expect(\n        typeof value === type, '[' + value + '] (' + id + ') is not a ' + type);\n  }\n\n  expect(!loadTimeData, 'should only include this file once');\n  loadTimeData = new LoadTimeData;\n\n  // Expose |loadTimeData| directly on |window|. This is only necessary by the\n  // auto-generated load_time_data.m.js, since within a JS module the scope is\n  // local.\n  window.loadTimeData = loadTimeData;\n\n/* #ignore */ console.warn('crbug/1173575, non-JS module files deprecated.');\n    \n document.addEventListener(\"DOMContentLoaded\", function(e) {\n    console.log(\"DOM CONTENT LOADED\")\n    // apparently for drop to work everywhere, you have to prevent default for enter/over/leave\n    // but we lose the cool icon hmm -- tried dropEffect \"copy\" everywhere, seems to work\n    // http://www.quirksmode.org/blog/archives/2009/09/the_html5_drag.html\n    document.addEventListener(\"dragover\", dragOver, false);\n    document.addEventListener(\"dragleave\", dragLeave, false);\n    document.addEventListener(\"dragenter\", dragEnter, false);\n    document.addEventListener(\"drop\", drop, false);\n})\n\nfunction dragOver(evt) {\n    console.log(arguments.callee.name)\n    evt.dataTransfer.dropEffect = 'copy'\n    evt.preventDefault();\n    return false;\n}\nfunction dragLeave(evt) {\n    console.log(arguments.callee.name)\n    evt.dataTransfer.dropEffect = 'copy'\n    evt.preventDefault();\n    return false;\n}\nfunction dragEnter(evt) {\n    console.log(arguments.callee.name)\n    evt.dataTransfer.dropEffect = 'copy'\n    evt.preventDefault();\n    return false;\n}\nfunction drop(evt) {\n    console.log(arguments.callee.name)\n    evt.dataTransfer.dropEffect = 'copy'\n    evt.preventDefault();\n    handleDrop(evt)\n    return false;\n}\n\nfunction handleDrop(evt) {\n        var items = evt.dataTransfer.items\n        if (items) {\n            for (var i=0; i<items.length; i++) {\n                item = items[i]\n                //console.log('drop found item',item)\n                if (item.kind == 'file') {\n                    var entry = item.webkitGetAsEntry()\n                    handleEntry(entry)\n                }\n            }\n        }\n}\n\nfunction handleEntry(entry) {\n    console.log('handle entry',entry)\n\n    function onread(evt) {\n        doupload(entry, evt.target.result)\n    }\n\n    var fr = new FileReader\n    fr.onload = fr.onerror = onread\n\n    entry.file( function(f) {\n        fr.readAsArrayBuffer(f)\n    })\n}\n\nfunction doupload(entry, data) {\n    console.log('doupload',entry,data)\n    fetch(window.location.pathname + encodeURIComponent(entry.name), {method:'PUT',body:data}).then(\n                                          function(r){console.log('upload resp',r); window.location.reload()}).catch(\n                                          function(e){console.log('upload err',e)})\n}\n</script><script>loadTimeData.data = {\"header\":\"Index of LOCATION\",\"headerDateModified\":\"Date Modified\",\"headerName\":\"Name\",\"headerSize\":\"Size\",\"language\":\"en\",\"listingParsingErrorBoxText\":\"Oh, no! This server is sending data Google Chrome can't understand. Please \\u003Ca href=\\\"http://code.google.com/p/chromium/issues/entry\\\">report a bug\\u003C/a>, and include the \\u003Ca href=\\\"LOCATION\\\">raw listing\\u003C/a>.\",\"parentDirText\":\"[parent directory]\",\"textdirection\":\"ltr\"};</script>\n";
}
