module Dustech.BookingApi.Utility


let uncurry f = fun (x, y) -> f x y

let curry f = fun x y -> f (x, y)