#tool nuget:?package=KuduSync.NET&version=1.3.1
#addin nuget:?package=Cake.Kudu&version=0.6.0
string          target      = Argument("target", "Build");
FilePath        ngPath      = Context.Tools.Resolve("ng.cmd");
FilePath        npmPath     = Context.Tools.Resolve("npm.cmd");
DirectoryPath   outputPath  = MakeAbsolute(Directory("./output"));

Action<FilePath, ProcessArgumentBuilder> Cmd => (path, args) => {
    var result = StartProcess(
        path,
        new ProcessSettings {
            Arguments = args
        });

    if(0 != result)
    {
        throw new Exception($"Failed to execute tool {path.GetFilename()} ({result})");
    }
};

Task("Install-AngularCLI")
    .Does(() => {
    if (ngPath != null && FileExists(ngPath))
    {
        Information("Found Angular CLI at {0}.", ngPath);
        return;
    }

    DirectoryPath ngDirectoryPath = MakeAbsolute(Directory("./Tools/ng"));
    
    EnsureDirectoryExists(ngDirectoryPath);

    Cmd(npmPath,
        new ProcessArgumentBuilder()
                .Append("install")
                .Append("--prefix")
                .AppendQuoted(ngDirectoryPath.FullPath)
                .Append("@angular/cli")
    );
    ngPath = Context.Tools.Resolve("ng.cmd");
});

Task("Clean")
    .Does( ()=> {
        CleanDirectory(outputPath);
});

Task("Install")
    .IsDependentOn("Clean")
    .Does( ()=> {
    Cmd(npmPath,
        new ProcessArgumentBuilder()
            .Append("install")
    );
});

Task("Build")
    .IsDependentOn("Install-AngularCLI")
    .IsDependentOn("Install")
    .Does( ()=> {
    Cmd(ngPath,
        new ProcessArgumentBuilder()
            .Append("build")
            .Append("--output-path")
            .AppendQuoted(outputPath.FullPath)
    );
});

Task("Publish")
    .IsDependentOn("Build")
    .Does( ()=> {
    Kudu.Sync(outputPath);
});

RunTarget(target);