
open Csv

let ( let* ) = Result.bind

let parse_float raw_float =
  raw_float
  |> float_of_string_opt
  |> Option.to_result ~none:`Invalid_float

let parse_asset_prices csv_str =
  let csv = Csv.of_string ~has_header:true csv_str in
  let headers = Csv.get_header csv |> List.tl in
  let rows = Csv.Rows.input_all csv in
  let init_map =
    List.fold_left
      (fun acc header -> Map.String.add header [] acc)
      Map.String.empty
      headers
  in
  let process_row map row =
    let* prices =
      List.tl (Csv.Row.to_list row)
      |> List.map parse_float
      |> List.fold_right
           (fun price acc ->
             let* prices = acc in
             let* price = price in
             Ok (price :: prices))
           (Ok [])
    in
    List.combine headers prices
    |> List.fold_left
         (fun acc (header, price) ->
           Map.String.update header
             (function
               | Some lst -> Some (price :: lst)
               | None -> Some [ price ])
             acc)
         map
    |> Ok
  in
  let* final_map =
    List.fold_left
      (fun acc row ->
        let* map = acc in
        process_row map row)
      (Ok init_map)
      rows
  in
  Ok (Map.String.map (fun lst -> List.rev lst) final_map)
