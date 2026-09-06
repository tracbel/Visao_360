// BinInspector — inventario SOMENTE LEITURA de binarios/configs.
//
// O que faz, por arquivo: tamanho, data, VersionInfo do PE, SHA256, arquitetura do PE
// e — quando o arquivo e um assembly .NET — nome, versao, TargetFramework, referencias
// de assembly e contagem de tipos publicos.
//
// IMPORTANTE: usa PEReader/MetadataReader (leitura de METADADOS). NUNCA chama
// Assembly.Load/LoadFrom, portanto nenhum codigo do binario inspecionado e executado.
// Nao descompila IL.
//
// Uso:
//   dotnet run --project BinInspector -- --root "\\host\c$\Vortice\TDev" --label TDev --out saida.csv
//   (pode repetir --root/--label; separe varios roots por ';')
//
// Opcoes:
//   --hash-ext  .exe,.dll,...   extensoes que recebem SHA256 (default abaixo)
//   --max-hash-mb N             acima disso o hash e pulado (default 40)

using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;

var roots = new List<(string Path, string Label)>();
string outCsv = "inventario.csv";
var hashExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    ".exe", ".dll", ".cll", ".ini", ".config", ".xml", ".apd", ".apl", ".app",
    ".dat", ".ocx", ".sys", ".bat", ".cmd", ".ps1", ".sql", ".json", ".ora", ".vlc"
};
long maxHashBytes = 40L * 1024 * 1024;
string pendingLabel = null;

for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--root":
            foreach (var r in args[++i].Split(';', StringSplitOptions.RemoveEmptyEntries))
                roots.Add((r.Trim(), pendingLabel ?? r.Trim()));
            pendingLabel = null;
            break;
        case "--label": pendingLabel = args[++i]; break;
        case "--out": outCsv = args[++i]; break;
        case "--max-hash-mb": maxHashBytes = long.Parse(args[++i]) * 1024 * 1024; break;
        case "--hash-ext":
            hashExt.Clear();
            foreach (var e in args[++i].Split(',', StringSplitOptions.RemoveEmptyEntries)) hashExt.Add(e.Trim());
            break;
    }
}

if (roots.Count == 0) { Console.Error.WriteLine("Informe ao menos um --root."); return 2; }

var linhas = new List<string[]>();
var cab = new[]
{
    "Origem","Pasta","Arquivo","Extensao","TamanhoBytes","ModificadoUtc","Sha256",
    "TipoDetectado","PeArquitetura","PeSubsystem","PeTimestampUtc",
    "FileVersion","ProductVersion","CompanyName","ProductName","FileDescription","Copyright",
    "AssemblyNome","AssemblyVersao","TargetFramework","TiposPublicos","QtdReferencias","Referencias","Observacao"
};

var sw = Stopwatch.StartNew();
foreach (var (root, label) in roots)
{
    if (!Directory.Exists(root)) { Console.Error.WriteLine($"[AVISO] inacessivel: {root}"); continue; }
    Console.Error.WriteLine($"[{label}] varrendo {root} ...");
    IEnumerable<string> arquivos;
    try { arquivos = Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories); }
    catch (Exception ex) { Console.Error.WriteLine($"[ERRO] {root}: {ex.Message}"); continue; }

    int n = 0;
    foreach (var f in arquivos)
    {
        n++;
        if (n % 200 == 0) Console.Error.WriteLine($"  ... {n} arquivos ({sw.Elapsed:mm\\:ss})");
        try { linhas.Add(Inspecionar(f, root, label, hashExt, maxHashBytes)); }
        catch (Exception ex)
        {
            var fi = new FileInfo(f);
            linhas.Add(new[]{ label, Path.GetDirectoryName(f) ?? "", Path.GetFileName(f), Path.GetExtension(f),
                "", "", "", "erro", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "",
                "falha ao inspecionar: " + ex.Message.Replace('\n',' ') });
        }
    }
    Console.Error.WriteLine($"[{label}] {n} arquivos.");
}

using (var w = new StreamWriter(outCsv, false, new UTF8Encoding(true)))
{
    w.WriteLine(string.Join(",", cab.Select(Csv)));
    foreach (var l in linhas) w.WriteLine(string.Join(",", l.Select(Csv)));
}
Console.Error.WriteLine($"OK -> {outCsv} ({linhas.Count} linhas, {sw.Elapsed:mm\\:ss})");
return 0;

static string Csv(string s)
{
    s ??= "";
    if (s.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0) return "\"" + s.Replace("\"", "\"\"") + "\"";
    return s;
}

static string[] Inspecionar(string f, string root, string label, HashSet<string> hashExt, long maxHash)
{
    var fi = new FileInfo(f);
    var ext = fi.Extension;
    string sha = "";
    string obs = "";

    if (hashExt.Contains(ext))
    {
        if (fi.Length <= maxHash)
        {
            using var fs = File.OpenRead(f);
            sha = Convert.ToHexString(SHA256.HashData(fs)).ToLowerInvariant();
        }
        else obs = "SHA256 nao calculado (arquivo acima do limite)";
    }
    else obs = "SHA256 nao calculado (extensao fora da lista)";

    string tipo = "outro", arq = "", subsys = "", peTs = "";
    string fv = "", pv = "", cn = "", pn = "", fd = "", cr = "";
    string asmNome = "", asmVer = "", tfm = "", tipos = "", qtdRef = "", refs = "";

    bool pePossivel = ext.Equals(".exe", StringComparison.OrdinalIgnoreCase)
        || ext.Equals(".dll", StringComparison.OrdinalIgnoreCase)
        || ext.Equals(".ocx", StringComparison.OrdinalIgnoreCase)
        || ext.Equals(".sys", StringComparison.OrdinalIgnoreCase);

    if (pePossivel)
    {
        try
        {
            var vi = System.Diagnostics.FileVersionInfo.GetVersionInfo(f);
            fv = vi.FileVersion ?? ""; pv = vi.ProductVersion ?? ""; cn = vi.CompanyName ?? "";
            pn = vi.ProductName ?? ""; fd = vi.FileDescription ?? ""; cr = vi.LegalCopyright ?? "";
        }
        catch { }

        try
        {
            using var fs = File.OpenRead(f);
            using var pe = new PEReader(fs);
            var h = pe.PEHeaders;
            arq = h.CoffHeader.Machine.ToString();
            subsys = h.PEHeader?.Subsystem.ToString() ?? "";
            peTs = DateTimeOffset.FromUnixTimeSeconds(h.CoffHeader.TimeDateStamp)
                     .UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            if (h.CorHeader != null && pe.HasMetadata)
            {
                tipo = ".NET";
                var mr = pe.GetMetadataReader();   // SO metadados; nenhum IL e executado
                if (mr.IsAssembly)
                {
                    var a = mr.GetAssemblyDefinition();
                    asmNome = mr.GetString(a.Name);
                    asmVer = a.Version.ToString();
                }
                else
                {
                    var m = mr.GetModuleDefinition();
                    asmNome = mr.GetString(m.Name) + " (modulo, sem manifesto)";
                }

                // TargetFrameworkAttribute
                tfm = LerTargetFramework(mr);

                int pub = 0;
                foreach (var th in mr.TypeDefinitions)
                {
                    var td = mr.GetTypeDefinition(th);
                    var vis = td.Attributes & System.Reflection.TypeAttributes.VisibilityMask;
                    if (vis == System.Reflection.TypeAttributes.Public
                        || vis == System.Reflection.TypeAttributes.NestedPublic) pub++;
                }
                tipos = pub.ToString(CultureInfo.InvariantCulture);

                var lista = new List<string>();
                foreach (var rh in mr.AssemblyReferences)
                {
                    var ar = mr.GetAssemblyReference(rh);
                    lista.Add($"{mr.GetString(ar.Name)} {ar.Version}");
                }
                lista.Sort(StringComparer.OrdinalIgnoreCase);
                qtdRef = lista.Count.ToString(CultureInfo.InvariantCulture);
                refs = string.Join(" | ", lista);
            }
            else tipo = "nativo (PE sem CLI header)";
        }
        catch (BadImageFormatException) { tipo = "nao-PE / formato desconhecido"; }
        catch (Exception ex) { tipo = "PE ilegivel"; obs = (obs + "; " + ex.Message).Trim(';', ' '); }
    }
    else
    {
        tipo = ext.ToLowerInvariant() switch
        {
            ".ini" or ".config" or ".xml" or ".json" or ".ora" => "configuracao",
            ".cll" or ".vlc" => "licenca Vortice",
            ".apd" or ".apl" or ".app" => "Gupta/Team Developer (p-code)",
            ".qrp" => "relatorio Gupta Report Builder",
            ".qvw" => "view/consulta Vortice",
            ".db" => "base local / cache",
            ".pdb" => "simbolos de depuracao",
            ".sql" => "script SQL",
            ".log" => "log",
            _ => "outro"
        };
    }

    return new[]
    {
        label,
        (Path.GetDirectoryName(f) ?? "").Replace(root, "<raiz>"),
        fi.Name, ext,
        fi.Length.ToString(CultureInfo.InvariantCulture),
        fi.LastWriteTimeUtc.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
        sha, tipo, arq, subsys, peTs,
        fv, pv, cn, pn, fd, cr,
        asmNome, asmVer, tfm, tipos, qtdRef, refs, obs
    };
}

static string LerTargetFramework(MetadataReader mr)
{
    foreach (var ch in mr.CustomAttributes)
    {
        var ca = mr.GetCustomAttribute(ch);
        string nome = NomeDoAtributo(mr, ca);
        if (nome != "TargetFrameworkAttribute") continue;
        try
        {
            var blob = mr.GetBlobReader(ca.Value);
            if (blob.ReadUInt16() != 1) return "";
            return blob.ReadSerializedString() ?? "";
        }
        catch { return ""; }
    }
    return "";
}

static string NomeDoAtributo(MetadataReader mr, CustomAttribute ca)
{
    try
    {
        if (ca.Constructor.Kind == HandleKind.MemberReference)
        {
            var m = mr.GetMemberReference((MemberReferenceHandle)ca.Constructor);
            if (m.Parent.Kind == HandleKind.TypeReference)
                return mr.GetString(mr.GetTypeReference((TypeReferenceHandle)m.Parent).Name);
        }
        else if (ca.Constructor.Kind == HandleKind.MethodDefinition)
        {
            var md = mr.GetMethodDefinition((MethodDefinitionHandle)ca.Constructor);
            return mr.GetString(mr.GetTypeDefinition(md.GetDeclaringType()).Name);
        }
    }
    catch { }
    return "";
}
