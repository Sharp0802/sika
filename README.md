# BlogMan

A cross-platform static blog webpage generator, written in C#.

## SYNOPSIS

```shell
blogman [COMMAND] [OPTION]...
```

## DESCRIPTION

You can create new project file with below command:

<pre>
blogman init --project <i>[your-project-file]</i>
</pre>

This command must be invoked at first.
Other commands depend on the project file created with this command.
After generating project file,
you can customize project with editing project file.

You can create new post file with below command:

<pre>
blogman new --project <i>[your-project-file]</i> --name <i>[new-post-name]</i>
</pre>

You can build a project file with below command:

<pre>
blogman build --project <i>[your-project-file]</i>
</pre>

After generating site, site files are stored into `site` folder.

You can get help message with below command:

<pre>
blogman --help
</pre>

Basically, `blogman` uses below file structure:

```
- project.blog.json
- build
    - sample-post.md
    - sample-meta.yaml
    - ...
- layout
    - sample-template.razor
    - ...
- post
    - sample-post.md
    - ...
- site
    - ...
```

You don't need to create these files and folders manually.
When you request `blogman` to build project or create new post file,
`blogman` automatically creates it.

Also, you don't have to create your own template file.
`blogman` provide you default template file.
You can use it by specifying post template to `default`:

```
---
...
Layout: default
...
---
...
```

## OPTIONS

- `init`
  - `--project`: The project name to create (**REQUIRED**)

- `new`
  - `--project`: The project file to build (**REQUIRED**)
  - `--name`: The name of new post (**REQUIRED**)


- `build`
    - `--project`: The project file to build (**REQUIRED**)

## SECURITY

`blogman` uses razor templating system.
Because of not only that razor template uses C# scripting internally
also that `RazorEngine` library has its own security vulnerability(remote code execution),
Using untrusted template file can be harmful to host computer.
Thus, do **NOT** use template file you cannot understand.
