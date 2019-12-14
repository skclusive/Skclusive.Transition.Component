Skclusive.Mobx.Component
=============================

Razor/Blazor Component library to be used with Skclusive.Mobx.Observable which is port of [MobX](https://github.com/mobxjs/mobx) for the C# language.

> Supercharge the state-management in your Blazor apps with Transparent Functional Reactive Programming (TFRP)

## Introduction

MobX is a state-management library that makes it simple to connect the
reactive data of your application with the UI. This wiring is completely automatic
and feels very natural. As the application-developer, you focus purely on what reactive-data
needs to be consumed in the UI (and elsewhere) without worrying about keeping the two
in sync.

It's not really magic but it does have some smarts around what is being consumed (**observables**)
and where (**reactions**), and automatically tracks it for you. When the _observables_
change, all _reactions_ are re-run. What's interesting is that these reactions can be anything from a simple
console log, a network call to re-rendering the UI.

> MobX has been a very effective library for the JavaScript
> apps and this port to the C# language aims to bring the same levels of productivity.

## Sample

The TransitionApp sample project has been published [here](https://skclusive.github.io/Skclusive.Mobx.Component/).

![Observable Counter in action](images/counter.gif)

### Installation

Add a reference to the library from [![NuGet](https://img.shields.io/nuget/v/Skclusive.Mobx.Component.svg)](https://www.nuget.org/packages/Skclusive.Mobx.Component/)


## Usage

Components with Skclusive.Mobx.Observable values can be created and used in as below.

```razor
@using Skclusive.Mobx.Component
@using Skclusive.Mobx.Observable

@inherits PureComponentBase

<h1>Observable Counter</h1>
<p>Current count: <Observable>@currentCount.Value</Observable></p>
<button class="btn btn-primary" @onclick="IncrementCount">
    Click me
</button>

@code {

    private IObservableValue<int> currentCount = ObservableValue<int>.From(0);

    private void IncrementCount()
    {
        currentCount.Value++;
    }
}

```

## Credits

This is an attempt to port [mobx](https://github.com/mobxjs/mobx) and [mobx-react](https://github.com/mobxjs/mobx-react) to Blazor.

## License

Skclusive.Mobx.Component is licensed under [MIT license](http://www.opensource.org/licenses/mit-license.php)
