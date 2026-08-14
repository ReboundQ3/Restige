using System.Diagnostics.CodeAnalysis;
using Robust.Client.UserInterface;

namespace Content.Client.Stylesheets;

public interface IStylesheetManager
{
    /// Sector Vestige styles: the default style! Use this for most UIs
    Stylesheet SheetSv { get; }

    /// Sector Vestige system styles: use this for any admin / debug menus
    Stylesheet SheetSvSystem { get; }

    /// Nanotrasen styles: upstream's default. Prefer <see cref="SheetSv"/>
    Stylesheet SheetNanotrasen { get; }

    ///
    /// System styles: use this for any admin / debug menus, and any odds and ends (like the changelog for some reason)
    ///
    Stylesheet SheetSystem { get; }


    [Obsolete("Update to use SheetNanotrasen instead")]
    Stylesheet SheetNano { get; }

    [Obsolete("Update to use SheetSystem instead")]
    Stylesheet SheetSpace { get; }

    /// get a stylesheet by name
    public bool TryGetStylesheet(string name, [MaybeNullWhen(false)]  out Stylesheet stylesheet);

    void Initialize();

    ///
    /// Sheetlets marked with CommonSheetlet that have not satisfied the type constraints of any stylesheet
    ///
    public HashSet<Type> UnusedSheetlets { get; }
}
