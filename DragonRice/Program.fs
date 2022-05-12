open DragonRice.WindowsInterop

[<EntryPoint>]
let main args =
    
    // Hide the taskbar

    printfn "Attempting to toggle taskbar..."
    let success = 
       WrapperMethods.findWindow (WrapperMethods.getWindowClassName >> (fun s -> s.Contains("Shell_TrayWnd")))
       |> Option.map WrapperMethods.trueHideWindow

    printfn "%O" success

    WrapperMethods.getAllDesktopWindows ()
    |> List.choose ((fun w -> if NativeMethods.IsWindowVisible w.RawPtr then Some w else None))
    |> List.map WrapperMethods.getWindowClassName
    |> List.iter (printfn "%s")

    System.Console.ReadKey() |> ignore

    0