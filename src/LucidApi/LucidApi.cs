using LucidStandardImport;

// namespace LucidApi 
// {
    // public class LucidApi
    // {
        // public void LucidStandardImport(LucidDocument document, 
    //     def upload_lucid_document(file_path, token, file_type, title, parent):
    // tok = token["access_token"]
    // """
    // Upload a .lucid file to Lucid's API.

    // Parameters:
    //     file_path (str): The path to the .lucid file on local disk.
    //     token (str): The bearer token for authorization.

    // Returns:
    //     response (requests.Response): The response object from the requests library.
    // """
    // url = "https://api.lucid.co/documents"
    // headers = {"Authorization": f"Bearer {tok}", "Lucid-Api-Version": "1"}

    // # Prepare the file and form data
    // with open(file_path, "rb") as file:
    //     # files = {"file": (lucid_file, zip_file, "x-application/vnd.lucid.standardImport")}

    //     files = {"file": ("data.lucid", file, file_type)}
    //     data = {
    //         "type": "x-application/vnd.lucid.standardImport",
    //         "title": title,
    //         "product": "lucidchart",
    //     }

    //     # Make the POST request with multipart/form-data
    //     response = requests.post(
    //         url, headers=headers, files=files, data=data, timeout=580
    //     )

    // return response

    // }
// }