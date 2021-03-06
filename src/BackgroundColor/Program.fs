﻿open System
open Aardvark.Base
open Aardvark.Base.Rendering
open Aardvark.Base.Incremental
open Aardvark.SceneGraph
open Aardvark.Application
open Aardvark.Application.WinForms

[<EntryPoint>]
let main argv =

    // initialize runtime system
    Ag.initialize(); Aardvark.Init()

    // create simple render window
    use app = new OpenGlApplication()
    let win = app.CreateSimpleRenderWindow()
    win.Text <- "Background Color (aardvark.docs)"

    // view, projection and default camera controllers
    let initialView = CameraView.lookAt (V3d(4, 3, 2)) V3d.Zero V3d.OOI
    let view = initialView |> DefaultCameraController.control win.Mouse win.Keyboard win.Time
    let proj = win.Sizes |> Mod.map (fun s -> Frustum.perspective 60.0 0.1 100.0 (float s.X / float s.Y))
    
    // define scene
    let sg =
        Sg.box' C4b.White Box3d.Unit 
            |> Sg.effect [
                DefaultSurfaces.trafo |> toEffect
                DefaultSurfaces.vertexColor |> toEffect
                DefaultSurfaces.simpleLighting |> toEffect
               ]
            |> Sg.viewTrafo (view |> Mod.map CameraView.viewTrafo)
            |> Sg.projTrafo (proj |> Mod.map Frustum.projTrafo)

    // background color
    let bgColor = Mod.init C4f.Black

    // animate background color
    let bgColorAnimation = async {
        let r = Random()
        while true do
            do! Async.Sleep 100
            let randomColor = C4f(r.NextDouble(), r.NextDouble(), r.NextDouble())
            transact ( fun () -> Mod.change bgColor randomColor )
    }
    
    Async.Start bgColorAnimation
    

    // specify render task(s)
    let task =
        [
            app.Runtime.CompileClear(win.FramebufferSignature, bgColor)
            app.Runtime.CompileRender(win.FramebufferSignature, sg)
        ]
        |> RenderTask.ofList
        //|> DefaultOverlays.withStatistics

    // start
    win.RenderTask <- task
    win.Run()
    0
