# ASP.NET Boilerplate

[![Build Status](https://github.com/yopichy/aspnetboilerplate/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/yopichy/aspnetboilerplate/actions/workflows/build-and-test.yml)
[![NuGet](https://img.shields.io/nuget/v/AbpLts.svg?style=flat-square)](https://www.nuget.org/packages/AbpLts)
[![NuGet Download](https://img.shields.io/nuget/dt/AbpLts.svg?style=flat-square)](https://www.nuget.org/packages/AbpLts)

> ### Long Term Support Update
> ASP.NET Boilerplate is no longer receiving new feature development, but it is maintained in an LTS mode for security patches only. See the original announcement at [read the end of life announcement](https://aspnetboilerplate.com/endofsupport?utm_source=referral&utm_medium=github&utm_campaign=github_zboilerplate_announcement_redirection).

> ### Migrating from Abp to AbpLts
> Migrating from the original `Abp` packages to `AbpLts` is straightforward. Simply replace all `Abp.*` package references in your `.csproj` files with the `AbpLts.*` equivalents and bump the version number:
>
> **Before:**
> ```xml
> <PackageReference Include="Abp.*" Version="10.3.0" />
> ```
>
> **After:**
> ```xml
> <PackageReference Include="AbpLts.*" Version="10.5.0" />
> ```
>
> The version is intentionally bumped from `10.3.0` to `10.5.0` to make the distinction between the original packages and the LTS fork clearly visible.
>
> **Versioning policy going forward:** The major version of `AbpLts` will follow the target .NET version. For example, when targeting .NET 11, the package version will be `AbpLts v11.x`.

## What is ABP?

[ASP.NET Boilerplate](https://aspnetboilerplate.com) is a general purpose **application framework** specially designed for new modern web applications. It uses already **familiar tools** and implements **best practices** around them to provide you a **SOLID development experience**.

ASP.NET Boilerplate works with the latest **ASP.NET Core** & **EF Core** but also supports ASP.NET MVC 5.x & EF 6.x as well.

###### Modular Design

Designed to be <a href="https://aspnetboilerplate.com/Pages/Documents/Module-System" target="_blank">**modular**</a> and **extensible**, ABP provides the infrastructure to build your own modules, too.

###### Multi-Tenancy

**SaaS** applications made easy! Integrated <a href="https://aspnetboilerplate.com/Pages/Documents/Multi-Tenancy" target="_blank">multi-tenancy</a> from database to UI.

###### Well-Documented

Comprehensive <a href="https://aspnetboilerplate.com/Pages/Documents" target="_blank">**documentation**</a> and quick start tutorials.

## How It Works

Don't Repeat Yourself! ASP.NET Boilerplate automates common software development tasks by convention. You focus on your business code!

![ASP.NET Boilerplate](doc/img/abp-concerns.png)

See the <a href="https://aspnetboilerplate.com/Pages/Documents/Introduction" target="_blank">Introduction</a> document for more details.

## Layered Architecture

ABP provides a layered architectural model based on **Domain Driven Design** and provides a **SOLID** model for your application.

![NLayer Architecture](doc/img/abp-nlayer-architecture.png)

See the <a href="https://aspnetboilerplate.com/Pages/Documents/NLayer-Architecture" target="_blank">NLayer Architecture</a> document for more details.

## Nuget Packages

ASP.NET Boilerplate is distributed as NuGet packages.

|Package|Status|
|:------|:-----:|
|AbpLts|[![NuGet version](https://badge.fury.io/nu/AbpLts.svg)](https://badge.fury.io/nu/AbpLts)|
|AbpLts.AspNetCore|[![NuGet version](https://badge.fury.io/nu/AbpLts.AspNetCore.svg)](https://badge.fury.io/nu/AbpLts.AspNetCore)|
|AbpLts.Web.Common|[![NuGet version](https://badge.fury.io/nu/AbpLts.Web.Common.svg)](https://badge.fury.io/nu/AbpLts.Web.Common)|
|AbpLts.Web.Resources|[![NuGet version](https://badge.fury.io/nu/AbpLts.Web.Resources.svg)](https://badge.fury.io/nu/AbpLts.Web.Resources)|
|AbpLts.EntityFramework.Common|[![NuGet version](https://badge.fury.io/nu/AbpLts.EntityFramework.Common.svg)](https://badge.fury.io/nu/AbpLts.EntityFramework.Common)|
|AbpLts.EntityFramework|[![NuGet version](https://badge.fury.io/nu/AbpLts.EntityFramework.svg)](https://badge.fury.io/nu/AbpLts.EntityFramework)|
|AbpLts.EntityFrameworkCore|[![NuGet version](https://badge.fury.io/nu/AbpLts.EntityFrameworkCore.svg)](https://badge.fury.io/nu/AbpLts.EntityFrameworkCore)|
|AbpLts.NHibernate|[![NuGet version](https://badge.fury.io/nu/AbpLts.NHibernate.svg)](https://badge.fury.io/nu/AbpLts.NHibernate)|
|AbpLts.Dapper|[![NuGet version](https://badge.fury.io/nu/AbpLts.Dapper.svg)](https://badge.fury.io/nu/AbpLts.Dapper)|
|AbpLts.FluentMigrator|[![NuGet version](https://badge.fury.io/nu/AbpLts.FluentMigrator.svg)](https://badge.fury.io/nu/AbpLts.FluentMigrator)|
|AbpLts.AspNetCore|[![NuGet version](https://badge.fury.io/nu/AbpLts.AspNetCore.svg)](https://badge.fury.io/nu/AbpLts.AspNetCore)|
|AbpLts.AspNetCore.SignalR|[![NuGet version](https://badge.fury.io/nu/AbpLts.AspNetCore.SignalR.svg)](https://badge.fury.io/nu/AbpLts.AspNetCore.SignalR)|
|AbpLts.AutoMapper|[![NuGet version](https://badge.fury.io/nu/AbpLts.AutoMapper.svg)](https://badge.fury.io/nu/AbpLts.AutoMapper)|
|AbpLts.HangFire|[![NuGet version](https://badge.fury.io/nu/AbpLts.HangFire.svg)](https://badge.fury.io/nu/AbpLts.HangFire)|
|AbpLts.HangFire.AspNetCore|[![NuGet version](https://badge.fury.io/nu/AbpLts.HangFire.AspNetCore.svg)](https://badge.fury.io/nu/AbpLts.HangFire.AspNetCore)|
|AbpLts.Castle.Log4Net|[![NuGet version](https://badge.fury.io/nu/AbpLts.Castle.Log4Net.svg)](https://badge.fury.io/nu/AbpLts.Castle.Log4Net)|
|AbpLts.RedisCache|[![NuGet version](https://badge.fury.io/nu/AbpLts.RedisCache.svg)](https://badge.fury.io/nu/AbpLts.RedisCache)|
|AbpLts.RedisCache.ProtoBuf|[![NuGet version](https://badge.fury.io/nu/AbpLts.RedisCache.ProtoBuf.svg)](https://badge.fury.io/nu/AbpLts.RedisCache.ProtoBuf)|
|AbpLts.MailKit|[![NuGet version](https://badge.fury.io/nu/AbpLts.MailKit.svg)](https://badge.fury.io/nu/AbpLts.MailKit)|
|AbpLts.Quartz|[![NuGet version](https://badge.fury.io/nu/AbpLts.Quartz.svg)](https://badge.fury.io/nu/AbpLts.Quartz)|
|AbpLts.TestBase|[![NuGet version](https://badge.fury.io/nu/AbpLts.TestBase.svg)](https://badge.fury.io/nu/AbpLts.TestBase)|
|AbpLts.AspNetCore.TestBase|[![NuGet version](https://badge.fury.io/nu/AbpLts.AspNetCore.TestBase.svg)](https://badge.fury.io/nu/AbpLts.AspNetCore.TestBase)|
|AbpLts.AspNetCore.OpenIddict|[![NuGet version](https://badge.fury.io/nu/AbpLts.AspNetCore.OpenIddict.svg)](https://badge.fury.io/nu/AbpLts.AspNetCore.OpenIddict)|

# Module Zero

## What is 'Module Zero'?

This is an <a href="https://aspnetboilerplate.com/" target="_blank">ASP.NET Boilerplate</a> module integrated with Microsoft <a href="https://docs.microsoft.com/en-us/aspnet/identity/overview/getting-started/introduction-to-aspnet-identity" target="_blank">ASP.NET Identity</a>.

Implements abstract concepts of ASP.NET Boilerplate framework:

* <a href="https://aspnetboilerplate.com/Pages/Documents/Setting-Management" target="_blank">Setting store</a>
* <a href="https://aspnetboilerplate.com/Pages/Documents/Audit-Logging" target="_blank">Audit log store</a>
* <a href="https://aspnetboilerplate.com/Pages/Documents/Background-Jobs-And-Workers" target="_blank">Background job store</a>
* <a href="https://aspnetboilerplate.com/Pages/Documents/Feature-Management" target="_blank">Feature store</a>
* <a href="https://aspnetboilerplate.com/Pages/Documents/Notification-System" target="_blank">Notification store</a>
* <a href="https://aspnetboilerplate.com/Pages/Documents/Authorization" target="_blank">Permission checker</a>

Also adds common enterprise application features:

* **<a href="https://aspnetboilerplate.com/Pages/Documents/Zero/User-Management" target="_blank">User</a>, <a href="https://aspnetboilerplate.com/Pages/Documents/Zero/Role-Management" target="_blank">Role</a> and <a href="https://aspnetboilerplate.com/Pages/Documents/Zero/Permission-Management" target="_blank">Permission</a>** management for applications that require authentication and authorization.
* **<a href="https://aspnetboilerplate.com/Pages/Documents/Zero/Tenant-Management" target="_blank">Tenant</a> and <a href="https://aspnetboilerplate.com/Pages/Documents/Zero/Edition-Management" target="_blank">Edition</a>** management for SaaS applications.
* **<a href="https://aspnetboilerplate.com/Pages/Documents/Zero/Organization-Units" target="_blank">Organization Units</a>** management.
* **<a href="https://aspnetboilerplate.com/Pages/Documents/Zero/Language-Management" target="_blank">Language and localization</a> text** management.
* **<a href="https://aspnetboilerplate.com/Pages/Documents/Zero/Identity-Server" target="_blank">Identity Server 4</a>** integration.

Module Zero packages define entities and implement base domain logic for these concepts.

## NuGet Packages

### ASP.NET Core Identity Packages

Packages integrated into <a href="https://docs.microsoft.com/en-us/aspnet/identity/overview/getting-started/introduction-to-aspnet-identity" target="_blank">ASP.NET Core Identity</a>.

|Package|Status|
|:------|:-----:|
|AbpLts.ZeroCore|[![NuGet version](https://badge.fury.io/nu/AbpLts.ZeroCore.svg)](https://badge.fury.io/nu/AbpLts.ZeroCore)|
|AbpLts.ZeroCore.EntityFrameworkCore|[![NuGet version](https://badge.fury.io/nu/AbpLts.ZeroCore.EntityFrameworkCore.svg)](https://badge.fury.io/nu/AbpLts.ZeroCore.EntityFrameworkCore)|

### Shared Packages

Shared packages between the Abp.ZeroCore.\* and Abp.Zero.\* packages.

|Package|Status|
|:------|:-----:|
|AbpLts.Zero.Common|[![NuGet version](https://badge.fury.io/nu/AbpLts.Zero.Common.svg)](https://badge.fury.io/nu/AbpLts.Zero.Common)|
|AbpLts.Zero.Ldap|[![NuGet version](https://badge.fury.io/nu/AbpLts.Zero.Ldap.svg)](https://badge.fury.io/nu/AbpLts.Zero.Ldap)|

## Startup Templates

You can create your project from startup templates to easily start with Module Zero:

* <a href="https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template-Angular" target="_blank">ASP.NET Core & Angular</a> based startup project.
* <a href="https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template-Core" target="_blank">ASP.NET Core MVC & jQuery</a> based startup project.
* <a href="https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template" target="_blank">ASP.NET Core MVC 5.x / AngularJS</a> based startup project.

A screenshot of the ASP.NET Core based startup template:

![](doc/img/module-zero-core-template-1.png)

## Links

* Web site & Documentation: https://aspnetboilerplate.com
* Questions & Answers: https://stackoverflow.com/questions/tagged/aspnetboilerplate?sort=newest

## Code of Conduct

This project has adopted the code of conduct defined by the Contributor Covenant to clarify expected behavior in our community.
For more information see the [.NET Foundation Code of Conduct](https://dotnetfoundation.org/code-of-conduct). 

### .NET Foundation

This project is supported by the [.NET Foundation](https://dotnetfoundation.org).

## License

[MIT](LICENSE).
