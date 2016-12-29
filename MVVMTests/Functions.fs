module Functions

open System 
open System.Linq.Expressions 

let inc x = x + 1
let dec x = x - 1

let getExpression<'t, 'p> (propertyName : string) =
    let p = Expression.Parameter(typedefof<'t>, "i")
    let v = typedefof<'t>
    let d = Expression.Property(p, propertyName)
    Expression.Lambda<Func<'t, 'p>>(d, [p])