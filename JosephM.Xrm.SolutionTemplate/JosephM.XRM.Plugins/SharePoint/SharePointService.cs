using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using $safeprojectname$.Core;
using $safeprojectname$.Xrm;

namespace $safeprojectname$.SharePoint
{
    /// <summary>
    /// A service class for performing logic
    /// </summary>
    public class SharePointService
    {
        private XrmService XrmService { get; set; }
        private ISharePointSettings SharepointSettings { get; set; }
        private bool CreateDynamicsFolderForTypes { get; set; }

        public SharePointService(ISharePointSettings sharepointSettings, XrmService xrmService)
        {
            XrmService = xrmService;
            CreateDynamicsFolderForTypes = true;
            SharepointSettings = sharepointSettings;
        }

        public void MoveFolder(string currentRelativeUrl, string newRelativeUrl)
        {
            var restQueryUrl = "_api/web/GetFolderByServerRelativePath(decodedurl='" + currentRelativeUrl + "')/moveto(newurl='" + newRelativeUrl + "')";
            var httpVerb = "POST";
            var response = GetResponseUTF8Data(restQueryUrl, httpVerb, null);
        }

        private string GenerateFolderName(string type, Guid id)
        {
            string name = null;
            var matchingConfigs = SharepointFolderConfigs.Where(c => c.Type == type).ToArray();
            var query = XrmService.BuildSourceQuery(type, matchingConfigs.SelectMany(c => c.FolderNameFields));
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition(XrmService.GetPrimaryKey(type), ConditionOperator.Equal, id);
            var targetRecord = XrmService.RetrieveFirst(query);
            foreach (var config in matchingConfigs)
            {
                foreach (var field in config.FolderNameFields)
                {
                    if (name != null)
                        name = name + " ";
                    var parseFieldParts = XrmService.GetTypeFieldPath(field, type).Last();
                    var fieldsType = parseFieldParts.Key;
                    var fieldsName = parseFieldParts.Value;
                    var fieldValue = targetRecord.GetField(field);
                    if (fieldValue == null)
                    {
                    }
                    else if (fieldValue is string)
                    {
                        name = ((string)fieldValue).Left(40);
                    }
                    else
                    {
                        throw new NotImplementedException("Folder name generation is only impleemnted for string type fields. Acutal type is " + fieldValue.GetType().Name);
                    }
                }
            }
            if (name == null)
            {
                var recordName = targetRecord.GetStringField(XrmService.GetPrimaryNameField(type));
                if (recordName == null)
                    recordName = string.Empty;

                const int maxLength = 75;
                var guidPart = id.ToString().ToUpper().Replace("-", "").Replace("{", "").Replace("}", "");
                var truncateName = recordName.Left((maxLength - guidPart.Length) - 1);
                name = truncateName + "_" + guidPart;
            }

            return name;
        }

        public void UploadDocument(string type, Guid id, string fileName, string document)
        {
            var getFolder = GetOrCreateSharepointFolder(type, id);
            UploadDocument(getFolder.RelativeUrl, fileName, document);
        }

        public virtual IEnumerable<SharepointFolderConfig> SharepointFolderConfigs
        {
            get
            {
                return new SharepointFolderConfig[0];
            }
        }

        public class SharepointFolderConfig
        {
            public string Type { get; set; }
            public string ParentType { get; set; }
            public string PathToParent { get; set; }
            public string PathToProductDivision { get; set; }
            public IEnumerable<ConditionExpression> Conditions { get; set; }
            public int? ActivePriority { get; set; }
            public IEnumerable<string> FolderNameFields { get; internal set; }
            public Func<string, Guid, Tuple<string, IDictionary<string, string>>> GetCustomProperties { get; set; }
        }

        public GetFolderResponse GetOrCreateSharepointFolder(string type, Guid id)
        {
            var relativeUrl = GetOrCreateDocumentFolderRelativeUrl(type, id);
            return new GetFolderResponse
            {
                RelativeUrl = relativeUrl,
                QualifiedUrl = SharepointSiteUri + "/" + relativeUrl
            };
        }

        public string UploadDocument(string relativeUrl, byte[] content)
        {
            var lastIndexOfPath = relativeUrl.LastIndexOf("/");
            if (lastIndexOfPath == -1)
                throw new Exception("Could not split url path for folder name");

            //documentFolderUrl = Uri.EscapeUriString(documentFolderUrl);
            var restQuery = string.Format("/_api/web/getfolderbyserverrelativeurl('{0}')/files/add(url='{1}',overwrite=true)", relativeUrl.Substring(0, lastIndexOfPath), relativeUrl.Substring(lastIndexOfPath + 1));
            var httpVerb = "POST";

            var response = GetResponseBase64Data(restQuery, httpVerb, Convert.ToBase64String(content));
            return string.Format("{0}/{1}", SharepointSiteUri, relativeUrl);
        }

        public string UploadDocument(string documentFolderUrl, string fileName, string fileContent, string prefix = null, string postfixUnique = null)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));
            if (documentFolderUrl == null)
                throw new ArgumentNullException(nameof(documentFolderUrl));
            if (fileContent == null)
                throw new ArgumentNullException(nameof(fileContent));

            string sharepointFileName = GenerateNewFileName(documentFolderUrl, fileName, prefix, postfixUnique);

            //documentFolderUrl = Uri.EscapeUriString(documentFolderUrl);
            var restQuery = string.Format("/_api/web/getfolderbyserverrelativeurl('{0}/{1}')/files/add(url='{2}',overwrite=true)", SharepointSiteUriLocalPath, documentFolderUrl, sharepointFileName);
            var httpVerb = "POST";

            var response = GetResponseBase64Data(restQuery, httpVerb, fileContent);
            return string.Format("{0}/{1}/{2}", SharepointSiteUri, documentFolderUrl, sharepointFileName);
        }

        public string GenerateNewFileName(string documentFolderUrl, string fileName, string prefix, string postfixUnique)
        {
            //okay so this mess is because sharepoint throws a 'bad request' error if the max filename length is exceeded
            //we need to ensure we are using a unique/new filename as well as not exceeding the max length

            //if we cannot just assume the filename is unique because the postfixUnique does not fit in the allowed file name length
            //then we will query the files and ensure whatever we use does not already exist

            prefix = prefix ?? "";
            postfixUnique = postfixUnique == null ? "" : "_" + postfixUnique;

            fileName = ReplaceInvalidChars(fileName);

            //experimented and discovered this max length
            //any more and get bad request due to exceeding max file name length
            //may include sharepoint url but is shorter in production than sandbox where tested so should work
            var maxLength = 164 - documentFolderUrl.Length;

            //truncate the fileName
            var lastIndexOfDot = fileName.LastIndexOf(".");
            var ext = lastIndexOfDot > -1
                ? fileName.Substring(lastIndexOfDot)
                : "";
            var fileNameNoExtention = fileName.Left(fileName.Length - ext.Length);

            //okay 15 characters I will deem as okay for as indication of what the file is
            var minimumNamePartLength = 15;

            string sharepointFileName = null;
            var maxFileNamePartLength = maxLength - ext.Length - postfixUnique.Length - prefix.Length;
            //if we can include at least minimumNamePartLength characters of the main fileName and include the prefix and unique part then
            //then no worries lets create it with that name
            if (maxFileNamePartLength >= minimumNamePartLength)
            {
                sharepointFileName = prefix + fileNameNoExtention.Left(maxFileNamePartLength) + postfixUnique + ext;
            }
            else
            {
                //otherwise this is the messy part we need to get a file name without the unique suffix but still ensure uniqueness of the file name we are creating
                //for duplicated generated names I am going to say we will have a suffix 001, 002, 003
                //this wil ensure uniqueness for 1000 documents. After that well lets just throw errors and address it if it ever happens
                var newNumberSuffixLength = 3;
                var newNumberSuffixLengthRecordCount = Convert.ToInt32(Math.Pow(10, newNumberSuffixLength));

                var maxLengthNewSuffix = maxLength - prefix.Length - ext.Length - newNumberSuffixLength;
                var namePartMaxNoSuffix = fileNameNoExtention.Left(maxLengthNewSuffix);
                var generatedCompressedNameNoSuffixOrExt = prefix + namePartMaxNoSuffix;
                var files = GetDocuments(documentFolderUrl, generatedCompressedNameNoSuffixOrExt, newNumberSuffixLengthRecordCount);
                var generatedUniqueName = false;
                for (var i = 0; i < newNumberSuffixLengthRecordCount; i++)
                {
                    var newNumberSuffix = i.ToString("0".ReplicateString(newNumberSuffixLength));
                    var newGeneratedComressedName = generatedCompressedNameNoSuffixOrExt + newNumberSuffix + ext;
                    if (!files.Any(n => n.Name == newGeneratedComressedName))
                    {
                        sharepointFileName = newGeneratedComressedName;
                        generatedUniqueName = true;
                        break;
                    }
                }
                if (!generatedUniqueName)
                {
                    throw new NotImplementedException("Error could not generate a new unique filename for the document. There are already too many documents with matching names");
                }
            }

            return sharepointFileName;
        }

        public string ReplaceInvalidChars(string fileName)
        {
            //replace any invalid chars
            //change to only spaces and alphanumeric as various characters were throwing errors
            var validNonLetterDigitsChars = " ._-"; 
            fileName = new string(fileName.Select(c => char.IsLetterOrDigit(c) || validNonLetterDigitsChars.Contains(c) ? c : '_').ToArray());

            return fileName.Trim();
        }


        public string GetOrCreateDocumentFolderRelativeUrl(string type, Guid id, string parentType = null, Guid? parentId = null)
        {
            //if already document location for this record
            var getUrl = GetDocumentFolderRelativeUrl(type, id);
            if (getUrl != null)
                return getUrl;

            var typeFolderParent = SharepointSite;
            //okay will if above didn't return then there is no folder yet for this record
            //so we need to create it
            string parentRelativeUrl = null;
            //if there is a parent then lets get or create the parent records folder
            //and store it is the typeFolderParent
            if (parentId.HasValue)
            {
                var parentPrimaryField = XrmService.GetPrimaryNameField(parentType);
                var parent = XrmService.Retrieve(parentType, parentId.Value, new[] { parentPrimaryField });
                parentRelativeUrl = GetOrCreateDocumentFolderRelativeUrl(parentType, parentId.Value);

                var parentRecordFolder = GetDocumentFolderForThisSite(parentType, parentId.Value);
                if (parentRecordFolder == null)
                    throw new Exception("Unexpected error - the parent folder was created but cannot be found");

                typeFolderParent = parentRecordFolder;
            }

            //query document folders for the record type
            var rootFolderQuery = XrmService.BuildQuery(Entities.sharepointdocumentlocation, null, new[] {
                new ConditionExpression(Fields.sharepointdocumentlocation_.relativeurl, ConditionOperator.Equal, type),
                new ConditionExpression(Fields.sharepointdocumentlocation_.parentsiteorlocation, ConditionOperator.Equal, typeFolderParent.Id)
            }, null);
            AddParentNodeLinks(rootFolderQuery);
            var matchingFolders = XrmService.RetrieveAll(rootFolderQuery);
            if (!matchingFolders.Any())
            {
                //if we didn't find a folder for the type in the root folder
                //then create it

                //then check if the folder has been created in sharepoint and if not create a new folder in sharepoint for this type (non-native integration folder)

                //if a root one first check if folder exists in sharepoint 
                if (!parentId.HasValue)
                {
                    var typeFolders = GetTypeFolders();
                    if (!typeFolders.Any(f => f.Name == type))
                    {
                        CreateDocumentLibrary(type);
                    }
                }
                else
                {
                    //else just create it
                    CreateFolder(parentRelativeUrl + "/" + type);
                }

                var sharepointLocation = new Entity(Entities.sharepointdocumentlocation);
                sharepointLocation.SetField(Fields.sharepointdocumentlocation_.relativeurl, type);
                sharepointLocation.SetField(Fields.sharepointdocumentlocation_.name, type);
                sharepointLocation.SetLookupField(Fields.sharepointdocumentlocation_.parentsiteorlocation, typeFolderParent);
                sharepointLocation.Id = XrmService.Create(sharepointLocation);

                rootFolderQuery = XrmService.BuildQuery(Entities.sharepointdocumentlocation, null, new[] {
                    new ConditionExpression(Fields.sharepointdocumentlocation_.sharepointdocumentlocationid, ConditionOperator.Equal, sharepointLocation.Id)
                    }, null);
                AddParentNodeLinks(rootFolderQuery);
                matchingFolders = XrmService.RetrieveAll(rootFolderQuery);
            }

            //if we matched a crm native integration document folder then generate the folder path (including parent folders)
            var typeFolderName = matchingFolders.Any()
                ? GetPathIncludingParentNodes(matchingFolders.First())
                : type;

            var entityFolderName = ReplaceInvalidChars(GenerateFolderName(type, id));
            CreateFolder(typeFolderName + "/" + entityFolderName);

            //if we matched to crm native integration for the type then create a crm document location record for the specific record
            if (matchingFolders.Any() && CreateDocumentLocationsForType(type))
            {
                var rootFolder = matchingFolders.First();
                var sharepointLocation = new Entity(Entities.sharepointdocumentlocation);
                sharepointLocation.SetField(Fields.sharepointdocumentlocation_.relativeurl, entityFolderName);
                sharepointLocation.SetField(Fields.sharepointdocumentlocation_.name, SharepointSite.GetStringField(Fields.sharepointsite_.name) + " Folder");
                sharepointLocation.SetLookupField(Fields.sharepointdocumentlocation_.regardingobjectid, id, type);
                sharepointLocation.SetLookupField(Fields.sharepointdocumentlocation_.parentsiteorlocation, rootFolder);
                var folderId = XrmService.Create(sharepointLocation);
            }

            var relativeUrl = typeFolderName + "/" + entityFolderName;
            return relativeUrl;
        }

        private bool CreateDocumentLocationsForType(string type)
        {
            return XrmService.GetEntityMetadata(type).IsDocumentManagementEnabled ?? false;
        }

        public void CreateDocumentLibrary(string newLibraryPath)
        {
            var data = "{'__metadata': { 'type': 'SP.List' }, 'AllowContentTypes': true, 'BaseTemplate': 101, 'ContentTypesEnabled': true, 'Description': 'My doc. lib. description', 'Title': '" + newLibraryPath + "' }";
            var restQueryUrl = "_api/web/lists";
            var httpVerb = "POST";
            var response = GetResponseUTF8Data(restQueryUrl, httpVerb, data);
        }

        public void CreateFolder(string newFolderPath)
        {
            var data = "{ '__metadata': { 'type': 'SP.Folder' }, 'ServerRelativeUrl': '" + SharepointSiteUriLocalPath + "/" + newFolderPath + "'}";
            var restQueryUrl = "_api/web/folders";
            var httpVerb = "POST";
            var response = GetResponseUTF8Data(restQueryUrl, httpVerb, data);
        }
    
        public void RenameFile(string fileName, string newFileName)
        {
            var restQueryUrl = "_api/web/GetFileByServerRelativePath(decodedurl='" + fileName + "')/copyto(newurl='" + newFileName + "')";
            var httpVerb = "POST";
            var response = GetResponseUTF8Data(restQueryUrl, httpVerb, null);
        }

        private void GetDeleteResponse(string restQuery, string httpVerb, string data)
        {
            var restQueryUrl = string.Format("{0}/{1}", SharepointSiteUri, restQuery);
            bool success = SpoAuthUtility.Create(new Uri(SharepointSiteUri), SharepointSettings.UserName, WebUtility.HtmlEncode(SharepointSettings.Password), false);
            if (success)
            {
                //headers
                var headers = new Dictionary<string, string>();
                //headers.Add("binaryStringResponseBody", "true");
                var requestDigest = SpoAuthUtility.GetRequestDigest();
                headers.Add("X-RequestDigest", requestDigest);
                headers.Add("X-HTTP-Method", "DELETE");

                // Send a json odata request
                byte[] result = HttpHelper.SendODataJsonRequest(
                    new Uri(restQueryUrl),
                    httpVerb, // reading data from SP through the rest api usually uses the GET verb
                    data == null ? null : Encoding.UTF8.GetBytes(data),
                    (HttpWebRequest)HttpWebRequest.Create(restQueryUrl),
                    SpoAuthUtility.Current, // pass in the helper object that allows us to make authenticated calls to SPO rest services
                    headers // specify that sharepoint needs to return uncontaminated bytes....
                );
                return;
            }
            else
            {
                throw new Exception("Unknown error generating sharepoint authentication");
            }
        }

        private string GetResponseBase64Data(string restQuery, string httpVerb, string data)
        {
            var restQueryUrl = string.Format("{0}/{1}", SharepointSiteUri, restQuery);
            bool success = SpoAuthUtility.Create(new Uri(SharepointSiteUri), SharepointSettings.UserName, WebUtility.HtmlEncode(SharepointSettings.Password), false);
            if (success)
            {
                //headers
                var headers = new Dictionary<string, string>();
                headers.Add("binaryStringResponseBody", "true");
                var requestDigest = SpoAuthUtility.GetRequestDigest();
                headers.Add("X-RequestDigest", requestDigest);


                // Send a json odata request
                byte[] result = HttpHelper.SendODataJsonRequest(
                    new Uri(restQueryUrl),
                    httpVerb, // reading data from SP through the rest api usually uses the GET verb
                    data == null ? null : Convert.FromBase64String(data),
                    (HttpWebRequest)HttpWebRequest.Create(restQueryUrl),
                    SpoAuthUtility.Current, // pass in the helper object that allows us to make authenticated calls to SPO rest services
                    headers // specify that sharepoint needs to return uncontaminated bytes....
                );

                return Encoding.UTF8.GetString(result, 0, result.Length);
            }
            else
            {
                throw new Exception("Unknown error generating sharepoint authentication");
            }
        }

        private string GetResponseUTF8Data(string restQuery, string httpVerb, string data)
        {
            var restQueryUrl = string.Format("{0}/{1}", SharepointSiteUri, restQuery);
            bool success = SpoAuthUtility.Create(new Uri(SharepointSiteUri), SharepointSettings.UserName, WebUtility.HtmlEncode(SharepointSettings.Password), false);
            if (success)
            {
                //headers
                var headers = new Dictionary<string, string>();
                headers.Add("binaryStringResponseBody", "true");
                var requestDigest = SpoAuthUtility.GetRequestDigest();
                headers.Add("X-RequestDigest", requestDigest);
                

                // Send a json odata request
                byte[] result = HttpHelper.SendODataJsonRequest(
                    new Uri(restQueryUrl),
                    httpVerb, // reading data from SP through the rest api usually uses the GET verb
                    data == null ? null : Encoding.UTF8.GetBytes(data),
                    (HttpWebRequest)HttpWebRequest.Create(restQueryUrl),
                    SpoAuthUtility.Current, // pass in the helper object that allows us to make authenticated calls to SPO rest services
                    headers // specify that sharepoint needs to return uncontaminated bytes....
                );

                return Encoding.UTF8.GetString(result, 0, result.Length);
            }
            else
            {
                throw new Exception("Unknown error generating sharepoint authentication");
            }
        }

        public string GetRawResponse(string folderRelativeUrl, string suffixPath)
        {
            var restQuery = string.Format("/_api/web/getfolderbyserverrelativeurl('{0}/{1}'){2}", SharepointSiteUriLocalPath, folderRelativeUrl, suffixPath);
            var httpVerb = "GET";

            var response = GetResponseUtf8String(restQuery, httpVerb, null);

            return response;
        }

        public void SetCustomProperties(string folderRelativeUrl, string type, IDictionary<string, string> propertyValues)
        {
            if (propertyValues == null)
                return;
            var jsonPart = string.Join(",", propertyValues.Select(pv => $"'{pv.Key}':'{pv.Value}'"));
            string json = "{ '__metadata': { 'type': '" + type + "' }, " + jsonPart + "}";
            var restQuery = string.Format("/_api/web/getfolderbyserverrelativeurl('{0}/{1}')/listitemallfields", SharepointSiteUriLocalPath, folderRelativeUrl);
            var httpVerb = "POST";
            GetResponse4(restQuery, httpVerb, json);
        }

        private void GetResponse4(string restQuery, string httpVerb, string data)
        {
            var restQueryUrl = string.Format("{0}/{1}", SharepointSiteUri, restQuery);
            bool success = SpoAuthUtility.Create(new Uri(SharepointSiteUri), SharepointSettings.UserName, WebUtility.HtmlEncode(SharepointSettings.Password), false);
            if (success)
            {
                //headers
                var headers = new Dictionary<string, string>();
                //headers.Add("binaryStringResponseBody", "true");
                var requestDigest = SpoAuthUtility.GetRequestDigest();
                headers.Clear();
                //headers.Add("Accept", "application/json;odata=verbose");
                headers.Add("X-RequestDigest", requestDigest);
                headers.Add("X-HTTP-Method", "MERGE");
                headers.Add("If-Match", "*");

                // Send a json odata request
                byte[] result = HttpHelper.SendODataJsonRequest(
                    new Uri(restQueryUrl),
                    httpVerb, // reading data from SP through the rest api usually uses the GET verb
                    data == null ? null : Encoding.UTF8.GetBytes(data),
                    (HttpWebRequest)HttpWebRequest.Create(restQueryUrl),
                    SpoAuthUtility.Current, // pass in the helper object that allows us to make authenticated calls to SPO rest services
                    headers // specify that sharepoint needs to return uncontaminated bytes....
                );
                return;
            }
            else
            {
                throw new Exception("Unknown error generating sharepoint authentication");
            }
        }

        public IEnumerable<SharepointFolder> GetTypeFolders()
        {
            var restQuery = string.Format("/_api/web/getfolderbyserverrelativeurl('{0}')/folders?$select=Name", SharepointSiteUriLocalPath);
            var httpVerb = "GET";
            string data = null;

            var response = GetResponseUtf8String(restQuery, httpVerb, data);
            var serialiser = new DataContractJsonSerializer(typeof(RootFolders));
            var deser = serialiser.ReadObject(GenerateStreamFromString(response));
            var deserAs = deser as RootFolders;

            return deserAs.data.results;
        }

        public IEnumerable<SharepointFolder> GetSubFolders(string folderRelativePath)
        {
            var restQuery = string.Format("/_api/web/getfolderbyserverrelativeurl('{0}/{1}')/folders?$select=Name", SharepointSiteUriLocalPath, folderRelativePath);
            var httpVerb = "GET";
            string data = null;

            var response = GetResponseUtf8String(restQuery, httpVerb, data);
            var serialiser = new DataContractJsonSerializer(typeof(RootFolders));
            var deser = serialiser.ReadObject(GenerateStreamFromString(response));
            var deserAs = deser as RootFolders;

            return deserAs.data.results;
        }

        [DataContract]
        public class RootFolders
        {
            [DataMember(Name = "d")]
            public d data { get; set; }

            [DataContract]
            public class d
            {
                [DataMember(Name = "results")]
                public List<SharepointFolder> results { get; set; }
            }
        }


        [DataContract]
        public class SharepointFolder
        {
            [DataMember(Name = "Name")]
            public string Name { get; set; }
        }

        public byte[] GetDocumentContent(string serverRelativeUrl)
        {
            var restQuery = string.Format("/_api/web/getfilebyserverrelativepath(decodedurl='{0}')/$value", serverRelativeUrl);
            var httpVerb = "GET";
            string data = null;

            return GetResponseByteArray(restQuery, httpVerb, data);
        }

        public void DeleteDocument(string serverRelativeUrl)
        {
            var restQuery = string.Format("/_api/web/getfilebyserverrelativepath(decodedurl='{0}')", serverRelativeUrl);
            var httpVerb = "POST";
            string data = null;
            GetDeleteResponse(restQuery, httpVerb, data);
        }

        public void DeleteFolder(string folderName)
        {
            var restQueryUrl = "_api/web/GetFolderByServerRelativeUrl('/" + SharepointSiteUriLocalPath + "/" + folderName + "')";
            var httpVerb = "POST";
            GetDeleteResponse(restQueryUrl, httpVerb, null);
        }


        public IEnumerable<SharepointDocument> GetDocuments(string folderRelativeUrl, string startsWith = null, int top = -1)
        {
            
            var startsWithArg = startsWith == null ? "" : "&$filter=substringof('" + startsWith + "',Name)";
            var tpArg = top > 0 ? "&$top=" + top : "";

            var serverRelativeFolder = Uri.EscapeUriString(string.Format("{0}/{1}", SharepointSiteUriLocalPath, folderRelativeUrl));

            var restQuery = string.Format("/_api/web/getfolderbyserverrelativeurl('{0}')/files?$select=Name{1}", serverRelativeFolder, startsWithArg);
            var httpVerb = "GET";
            string data = null;

            var response = GetResponseUtf8String(restQuery, httpVerb, data);
            var serialiser = new DataContractJsonSerializer(typeof(RootDocuments));
            var deser = serialiser.ReadObject(GenerateStreamFromString(response));
            var deserAs = deser as RootDocuments;

            return deserAs.data.results;
        }

        private byte[] GetResponseByteArray(string restQuery, string httpVerb, string data)
        {
            var restQueryUrl = string.Format("{0}/{1}", SharepointSiteUri, restQuery);
            bool success = SpoAuthUtility.Create(new Uri(SharepointSiteUri), SharepointSettings.UserName, WebUtility.HtmlEncode(SharepointSettings.Password), false);
            if (success)
            {
                //headers
                var headers = new Dictionary<string, string>();
                headers.Add("binaryStringResponseBody", "true");

                // Send a json odata request
                byte[] result = HttpHelper.SendODataJsonRequest(
                    new Uri(restQueryUrl),
                    httpVerb, // reading data from SP through the rest api usually uses the GET verb
                    data == null ? null : Encoding.ASCII.GetBytes(data),
                    (HttpWebRequest)HttpWebRequest.Create(restQueryUrl),
                    SpoAuthUtility.Current, // pass in the helper object that allows us to make authenticated calls to SPO rest services
                    headers // specify that sharepoint needs to return uncontaminated bytes....
                );

                return result;
            }
            else
            {
                throw new Exception("Unknown error generating sharepoint authentication");
            }
        }

        private string GetResponseUtf8String(string restQuery, string httpVerb, string data)
        {
            var bytes = GetResponseByteArray(restQuery, httpVerb, data);
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }

        private string GetDocumentFolderRelativeUrl(string regardingType, Guid regardingObjectId)
        {
            var documentFolderForThisSite = GetDocumentFolderForThisSite(regardingType, regardingObjectId);
            return documentFolderForThisSite == null ? null : GetPathIncludingParentNodes(documentFolderForThisSite);
        }

        private Entity GetDocumentFolderForThisSite(string regardingType, Guid regardingObjectId)
        {
            var query = XrmService.BuildQuery(Entities.sharepointdocumentlocation, null, new[] { new ConditionExpression(Fields.sharepointdocumentlocation_.regardingobjectid, ConditionOperator.Equal, regardingObjectId) }, null);
            AddParentNodeLinks(query);

            var results = XrmService.RetrieveAll(query)
                .Where(e => e.GetLookupType(Fields.sharepointdocumentlocation_.regardingobjectid) == regardingType);

            Entity documentFolderForThisSite = null;

            foreach (var result in results)
            {
                if (XrmEntity.GetLookupGuid(result.GetField("LINK1." + Fields.sharepointdocumentlocation_.parentsiteorlocation)) == SharepointSite.Id
                    || XrmEntity.GetLookupGuid(result.GetField("LINK2." + Fields.sharepointdocumentlocation_.parentsiteorlocation)) == SharepointSite.Id
                    || XrmEntity.GetLookupGuid(result.GetField("LINK3." + Fields.sharepointdocumentlocation_.parentsiteorlocation)) == SharepointSite.Id)
                {
                    documentFolderForThisSite = result;
                }
            }

            return documentFolderForThisSite;
        }

        private static string GetPathIncludingParentNodes(Entity firstResult)
        {
            var url = new StringBuilder();
            if (firstResult.GetField("LINK3." + Fields.sharepointdocumentlocation_.relativeurl) != null)
                url.Append(firstResult.GetField("LINK3." + Fields.sharepointdocumentlocation_.relativeurl) + "/");
            if (firstResult.GetField("LINK2." + Fields.sharepointdocumentlocation_.relativeurl) != null)
                url.Append(firstResult.GetField("LINK2." + Fields.sharepointdocumentlocation_.relativeurl) + "/");
            if (firstResult.GetField("LINK1." + Fields.sharepointdocumentlocation_.relativeurl) != null)
                url.Append(firstResult.GetField("LINK1." + Fields.sharepointdocumentlocation_.relativeurl) + "/");
            url.Append(firstResult.GetStringField(Fields.sharepointdocumentlocation_.relativeurl));
            var documentFolderUrl = url.ToString();
            return documentFolderUrl;
        }

        private void AddParentNodeLinks(QueryExpression query)
        {
            var parentLink1 = query.AddLink(Entities.sharepointdocumentlocation, Fields.sharepointdocumentlocation_.parentsiteorlocation, Fields.sharepointdocumentlocation_.sharepointdocumentlocationid, JoinOperator.LeftOuter);
            parentLink1.EntityAlias = "LINK1";
            parentLink1.Columns = XrmService.CreateColumnSet(null);
            var parentLink2 = parentLink1.AddLink(Entities.sharepointdocumentlocation, Fields.sharepointdocumentlocation_.parentsiteorlocation, Fields.sharepointdocumentlocation_.sharepointdocumentlocationid, JoinOperator.LeftOuter);
            parentLink2.EntityAlias = "LINK2";
            parentLink2.Columns = XrmService.CreateColumnSet(null);
            var parentLink3 = parentLink2.AddLink(Entities.sharepointdocumentlocation, Fields.sharepointdocumentlocation_.parentsiteorlocation, Fields.sharepointdocumentlocation_.sharepointdocumentlocationid, JoinOperator.LeftOuter);
            parentLink3.EntityAlias = "LINK3";
            parentLink3.Columns = XrmService.CreateColumnSet(null);
        }

        private Entity _sharepointSite;
        private Entity SharepointSite
        {
            get
            {
                if (_sharepointSite == null)
                {
                    _sharepointSite = XrmService.GetFirst(Entities.sharepointsite);
                }
                return _sharepointSite;
            }
        }

        public string SharepointSiteUri
        {
            get
            {
                return SharepointSite.GetStringField(Fields.sharepointsite_.absoluteurl);
            }
        }

        private string SharepointSiteUriLocalPath
        {
            get
            {
                var localPath = new Uri(SharepointSiteUri).LocalPath;
                return localPath == @"/" ? "" : localPath;
            }
        }

        [DataContract]
        public class RootDocuments
        {
            [DataMember(Name = "d")]
            public d data { get; set; }

            [DataContract]
            public class d
            {
                [DataMember(Name = "results")]
                public List<SharepointDocument> results { get; set; }
            }
        }


        [DataContract]
        public class SharepointDocument
        {
            [DataMember(Name = "Name")]
            public string Name { get; set; }
        }

        //pretty much all this remaining code was taken out
        //of a sample project someone posted online
        //just extended it with the code above 

        public static Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static class HttpHelper
        {
            /// <summary>
            /// Sends a JSON OData request appending SPO auth cookies to the request header.
            /// </summary>
            /// <param name="uri">The request uri</param>
            /// <param name="method">The http method</param>
            /// <param name="requestContent">A stream containing the request content</param>
            /// <param name="clientHandler">The request client handler</param>
            /// <param name="authUtility">An instance of the auth helper to perform authenticated calls to SPO</param>
            /// <param name="headers">The http headers to append to the request</param>
            public static byte[] SendODataJsonRequest(Uri uri, String method, byte[] requestContent, HttpWebRequest clientHandler, SpoAuthUtility authUtility, Dictionary<string, string> headers = null)
            {
                if (clientHandler.CookieContainer == null)
                    clientHandler.CookieContainer = new CookieContainer();

                CookieContainer cookieContainer = authUtility.GetCookieContainer(); // get the auth cookies from SPO after authenticating with Microsoft Online Services STS

                foreach (Cookie c in cookieContainer.GetCookies(uri))
                {
                    clientHandler.CookieContainer.Add(uri, c); // apppend SPO auth cookies to the request
                }

                return SendHttpRequest(
                    uri,
                    method,
                    requestContent,
                    "application/json;odata=verbose;charset=utf-8", // the http content type for the JSON flavor of SP REST services 
                    clientHandler,
                    headers);
            }


            /// <summary>
            /// Sends an http request to the specified uri and returns the response as a byte array 
            /// </summary>
            /// <param name="uri">The request uri</param>
            /// <param name="method">The http method</param>
            /// <param name="requestContent">A stream containing the request content</param>
            /// <param name="contentType">The content type of the http request</param>
            /// <param name="clientHandler">The request client handler</param>
            /// <param name="headers">The http headers to append to the request</param>
            public static byte[] SendHttpRequest(Uri uri, String method, byte[] requestContent = null, string contentType = null, HttpWebRequest clientHandler = null, Dictionary<string, string> headers = null)
            {
                HttpWebRequest request = clientHandler == null ? (HttpWebRequest)HttpWebRequest.Create(uri) : clientHandler;

                byte[] responseStream = null;
                request.Method = method;
                request.Accept = contentType;
                request.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)"; // This must be here as you will receive 403 otherwise
                request.AllowAutoRedirect = false; // This is key, otherwise it will redirect to failed login SP page

                // append additional headers to the request
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        if (request.Headers.AllKeys.Contains(header.Key))
                        {
                            request.Headers.Remove(header.Key);
                        }

                        request.Headers.Add(header.Key, header.Value);
                    }
                }

                if (requestContent != null && (method == "POST" || method == "PUT" || method == "DELETE"))
                {
                    if (!string.IsNullOrEmpty(contentType))
                    {
                        request.ContentType = contentType; // if the request has a body set the MIME type
                    }

                    request.ContentLength = requestContent.Length;
                    using (Stream s = request.GetRequestStream())
                    {
                        s.Write(requestContent, 0, requestContent.Length);
                        s.Close();

                    }

                }
                // for all other operations, force the contentlength to be 0. Otherwise you might get an 411 error
                else request.ContentLength = 0;

                // Get the response stream and turn it into a byte[]
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    CopyStream(response.GetResponseStream(), memoryStream);
                    if (memoryStream.Length > 0)
                    {
                        responseStream = memoryStream.ToArray();
                    }
                }

                return responseStream;
            }


            /// <summary>
            /// Copy one stream to the other (this is done in small 32KB chunks)
            /// </summary>
            /// <param name="source">incoming stream</param>
            /// <param name="destination">outgoing stream</param>
            private static void CopyStream(Stream source, Stream destination)
            {
                byte[] buffer = new byte[32768];
                int bytesRead;
                do
                {
                    bytesRead = source.Read(buffer, 0, buffer.Length);
                    destination.Write(buffer, 0, bytesRead);
                } while (bytesRead != 0);
            }

        }

        internal class SamlSecurityToken
        {
            public byte[] Token
            {
                get;
                set;
            }

            public DateTime Expires
            {
                get;
                set;
            }
        }

        internal class SPOAuthCookies
        {
            public string FedAuth
            {
                get;
                set;
            }

            public string RtFA
            {
                get;
                set;
            }

            public Uri Host
            {
                get;
                set;
            }

            public DateTime Expires
            {
                get;
                set;
            }
        }

        public class SpoAuthUtility
        {
            Uri spSiteUrl;
            string username;
            string password;
            Uri adfsIntegratedAuthUrl;
            Uri adfsAuthUrl;
            bool useIntegratedWindowsAuth;
            static SpoAuthUtility current;
            CookieContainer cookieContainer;
            SamlSecurityToken stsAuthToken;


            const string msoStsUrl = "https://login.microsoftonline.com/extSTS.srf";
            const string msoLoginUrl = "https://login.microsoftonline.com/login.srf";
            const string msoHrdUrl = "https://login.microsoftonline.com/GetUserRealm.srf";
            const string wsse = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";
            const string wsu = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";
            const string wst = "http://schemas.xmlsoap.org/ws/2005/02/trust";
            const string saml = "urn:oasis:names:tc:SAML:1.0:assertion";
            const string spowssigninUri = "_forms/default.aspx?wa=wsignin1.0";
            const string contextInfoQuery = "_api/contextinfo";

            public static bool Create(Uri spSiteUrl, string username, string password, bool useIntegratedWindowsAuth)
            {
                var utility = new SpoAuthUtility(spSiteUrl, username, password, useIntegratedWindowsAuth);
                CookieContainer cc = utility.GetCookieContainer();
                var cookies = from Cookie c in cc.GetCookies(spSiteUrl) where c.Name == "FedAuth" select c;
                if (cookies.Count() > 0)
                {
                    current = utility;
                    return true;
                }
                else
                    throw new Exception("Could not retrieve Auth cookies");
            }

            private SpoAuthUtility(Uri spSiteUrl, string username, string password, bool useIntegratedWindowsAuth)
            {
                this.spSiteUrl = spSiteUrl;
                this.username = username;
                this.password = password;
                this.useIntegratedWindowsAuth = useIntegratedWindowsAuth;

                stsAuthToken = new SamlSecurityToken();
            }

            public static SpoAuthUtility Current
            {
                get
                {
                    return current;
                }
            }

            public Uri SiteUrl
            {
                get
                {
                    return this.spSiteUrl;
                }
            }

            /// <summary>
            /// The method will request the SP ContextInfo and return its FormDigestValue as a String
            /// The FormDigestValue is a second layer of authentication required for several REST queries
            /// </summary>
            /// <returns>FormDigestValue</returns>
            public static string GetRequestDigest()
            {
                string digest = String.Empty;
                Uri url = new Uri(String.Format("{0}/{1}", SpoAuthUtility.Current.SiteUrl, contextInfoQuery));
                byte[] result = HttpHelper.SendODataJsonRequest(
                  url,
                  "POST", // Retrieving the Context Info needs a POST Method 
                  new byte[0],
                  (HttpWebRequest)HttpWebRequest.Create(url),
                  SpoAuthUtility.Current // pass in the helper object that allows us to make authenticated calls to SPO rest services
                  );

                var toString = Encoding.UTF8.GetString(result, 0, result.Length);

                // use the DataContractJsonSerializer instead of the JavascriptSerializer (system.web.extension.dll cannot be used in the sandbox)
                MemoryStream stream = new MemoryStream(result);
                stream.Position = 0;

                DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(ContextInfo));
                ContextInfo ci = (ContextInfo)json.ReadObject(stream);

                //ContextInfo ci = (ContextInfo)json.Deserialize(Encoding.UTF8.GetString(result, 0, result.Length), typeof(ContextInfo));
                digest = (ci != null) ? ci.data.ContextInfo.FormDigestValue : String.Empty;

                return digest;
            }

            /// <summary>
            /// Helper class to handle ContextInfo JSON Deserialisation
            /// </summary>
            [DataContract]
            public class ContextInfo
            {
                [DataMember(Name = "d")]
                public d data { get; set; }

                [DataContract]
                public class d
                {
                    [DataMember(Name = "GetContextWebInformation")]
                    public GetContextWebInformation ContextInfo { get; set; }
                }

                [DataContract]
                public class GetContextWebInformation
                {
                    [DataMember(Name = "FormDigestValue")]
                    public string FormDigestValue { get; set; }
                }
            }

            public CookieContainer GetCookieContainer()
            {
                if (stsAuthToken != null)
                {
                    if (DateTime.Now > stsAuthToken.Expires)
                    {
                        this.stsAuthToken = GetMsoStsSAMLToken();

                        // Check if we have a auth token
                        if (this.stsAuthToken != null)
                        {
                            SPOAuthCookies cookies = GetSPOAuthCookies(this.stsAuthToken);
                            CookieContainer cc = new CookieContainer();

                            Cookie samlAuthCookie = new Cookie("FedAuth", cookies.FedAuth)
                            {
                                Path = "/",
                                Expires = this.stsAuthToken.Expires,
                                Secure = cookies.Host.Scheme.Equals("https"),
                                HttpOnly = true,
                                Domain = cookies.Host.Host
                            };

                            cc.Add(this.spSiteUrl, samlAuthCookie);

                            Cookie rtFACookie = new Cookie("rtFA", cookies.RtFA)
                            {
                                Path = "/",
                                Expires = this.stsAuthToken.Expires,
                                Secure = cookies.Host.Scheme.Equals("https"),
                                HttpOnly = true,
                                Domain = cookies.Host.Host
                            };

                            cc.Add(this.spSiteUrl, rtFACookie);

                            this.cookieContainer = cc;
                        }
                    }
                }

                return this.cookieContainer;
            }

            private SPOAuthCookies GetSPOAuthCookies(SamlSecurityToken stsToken)
            {
                // signs in to SPO with the security token issued by MSO STS and gets the fed auth cookies
                // the fed auth cookie needs to be attached to all SPO REST services requests
                SPOAuthCookies spoAuthCookies = new SPOAuthCookies();

                Uri siteUri = this.spSiteUrl;
                Uri wsSigninUrl = new Uri(String.Format("{0}://{1}/{2}", siteUri.Scheme, siteUri.Authority, spowssigninUri));

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(wsSigninUrl);
                request.CookieContainer = new CookieContainer();

                byte[] responseData = HttpHelper.SendHttpRequest(
                    wsSigninUrl,
                    "POST",
                    stsToken.Token,
                    "application/x-www-form-urlencoded",
                    request,
                    null);

                if (request != null && responseData != null)
                {
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        spoAuthCookies.FedAuth = (response.Cookies["FedAuth"] != null) ? response.Cookies["FedAuth"].Value : null;
                        spoAuthCookies.RtFA = (response.Cookies["rtFA"] != null) ? response.Cookies["rtFA"].Value : null;
                        spoAuthCookies.Expires = stsToken.Expires;
                        spoAuthCookies.Host = wsSigninUrl;
                    }
                }

                return spoAuthCookies;
            }

            private Uri GetAdfsAuthUrl()
            {
                Uri corpAdfsProxyUrl = null;

                Uri msoHrdUri = new Uri(msoHrdUrl);
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(msoHrdUri);
                // make a post request with the user's login name to 
                // MSO HRD (Home Realm Discovery) service so it can find 
                // out the url of the federation service (corporate ADFS) 
                // responsible for authenticating the user

                byte[] response = HttpHelper.SendHttpRequest(
                     new Uri(msoHrdUrl),
                     "POST",
                     Encoding.UTF8.GetBytes(String.Format("handler=1&login={0}", this.username)), // pass in the login name in the body of the form
                     "application/x-www-form-urlencoded",
                     request);

                // convert byte[] to string
                STSInfo info = new STSInfo();
                if (response != null)
                {
                    string s = GetTokenFromJson(UTF8Encoding.UTF8.GetString(response), "AuthURL");
                    if (!string.IsNullOrEmpty(s)) info.AuthURL = s;
                }

                if (info != null && !String.IsNullOrEmpty(info.AuthURL))
                {
                    corpAdfsProxyUrl = new Uri(info.AuthURL);
                }


                return corpAdfsProxyUrl;
            }

            /// <summary>
            /// Some JSON objects cannot be deserialized.
            /// Then we do this by hand
            /// </summary>
            /// <param name="json">json response</param>
            /// <param name="tokenName">name of the token</param>
            /// <returns>value of token</returns>
            private string GetTokenFromJson(string json, string tokenName)
            {
                tokenName = "\"" + tokenName + "\":";
                if (json != null)
                {
                    string[] tokens = json.Split(',');
                    foreach (string token in tokens)
                    {
                        if (token.Trim().StartsWith(tokenName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            return token.Replace(tokenName, "").Replace("\"", "").Trim();
                        }
                    }
                }
                return null;
            }


            /// <summary>
            /// Helper class to handle STS Info JSON Deserialisation
            /// </summary>
            [Serializable]
            class STSInfo
            {
                public String AuthURL { get; set; }
            }



            private string GetAdfsSAMLTokenUsernamePassword()
            {
                // makes a seurity token request to the corporate ADFS proxy usernamemixed endpoint using
                // the user's corporate credentials. The logon token is used to talk to MSO STS to get
                // an O365 service token that can then be used to sign into SPO.
                string samlAssertion = null;

                // the corporate ADFS proxy endpoint that issues SAML seurity tokens given username/password credentials 
                string stsUsernameMixedUrl = String.Format("https://{0}/adfs/services/trust/2005/usernamemixed/", adfsAuthUrl.Host);

                // generate the WS-Trust security token request SOAP message passing in the user's corporate credentials 
                // and the site we want access to. We send the token request to the corporate ADFS proxy usernamemixed endpoint.
                byte[] requestBody = Encoding.UTF8.GetBytes(ParameterizeSoapRequestTokenMsgWithUsernamePassword(
                    "urn:federation:MicrosoftOnline", // we are requesting a logon token to talk to the Microsoft Federation Gateway
                    this.username,
                    this.password,
                    stsUsernameMixedUrl));

                try
                {
                    Uri stsUrl = new Uri(stsUsernameMixedUrl);
                    HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(stsUrl);

                    byte[] responseData = HttpHelper.SendHttpRequest(
                        stsUrl,
                        "POST",
                        requestBody,
                        "application/soap+xml; charset=utf-8",
                        request,
                        null);

                    if (responseData != null)
                    {
                        StreamReader sr = new StreamReader(new MemoryStream(responseData), Encoding.GetEncoding("utf-8"));
                        XPathNavigator nav = new XPathDocument(sr).CreateNavigator();
                        XmlNamespaceManager nsMgr = new XmlNamespaceManager(nav.NameTable);
                        nsMgr.AddNamespace("t", "http://schemas.xmlsoap.org/ws/2005/02/trust");
                        XPathNavigator requestedSecurityToken = nav.SelectSingleNode("//t:RequestedSecurityToken", nsMgr);

                        // Ensure whitespace is reserved
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(requestedSecurityToken.InnerXml);
                        doc.PreserveWhitespace = true;
                        samlAssertion = doc.InnerXml;
                    }

                }
                catch
                {
                    // we failed to sign the user using corporate credentials

                }

                return samlAssertion;
            }

            private string GetAdfsSAMLTokenWinAuth()
            {
                // makes a seurity token request to the corporate ADFS proxy integrated auth endpoint.
                // If the user is logged on to a machine joined to the corporate domain with her Windows credentials and connected
                // to the corporate network Kerberos automatically takes care of authenticating the security token 
                // request to ADFS.
                // The logon token is used to talk to MSO STS to get an O365 service token that can then be used to sign into SPO.

                string samlAssertion = null;

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(this.adfsIntegratedAuthUrl);
                request.UseDefaultCredentials = true; // use the default credentials so Kerberos can take care of authenticating our request

                byte[] responseData = HttpHelper.SendHttpRequest(
                    this.adfsIntegratedAuthUrl,
                    "GET",
                    null,
                    "text/html; charset=utf-8",
                    request);


                if (responseData != null)
                {
                    try
                    {
                        StreamReader sr = new StreamReader(new MemoryStream(responseData), Encoding.GetEncoding("utf-8"));
                        XPathNavigator nav = new XPathDocument(sr).CreateNavigator();
                        XPathNavigator wresult = nav.SelectSingleNode("/html/body/form/input[@name='wresult']");
                        if (wresult != null)
                        {
                            string RequestSecurityTokenResponseText = wresult.GetAttribute("value", "");

                            sr = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(RequestSecurityTokenResponseText)));
                            nav = new XPathDocument(sr).CreateNavigator();
                            XmlNamespaceManager nsMgr = new XmlNamespaceManager(nav.NameTable);
                            nsMgr.AddNamespace("t", "http://schemas.xmlsoap.org/ws/2005/02/trust");
                            XPathNavigator requestedSecurityToken = nav.SelectSingleNode("//t:RequestedSecurityToken", nsMgr);

                            // Ensure whitespace is reserved
                            XmlDocument doc = new XmlDocument();
                            doc.LoadXml(requestedSecurityToken.InnerXml);
                            doc.PreserveWhitespace = true;
                            samlAssertion = doc.InnerXml;
                        }

                    }
                    catch
                    {
                        // we failed to sign the user using integrated Windows Auth

                    }
                }


                return samlAssertion;

            }


            private SamlSecurityToken GetMsoStsSAMLToken()
            {
                // Makes a request that conforms with the WS-Trust standard to 
                // Microsoft Online Services Security Token Service to get a SAML
                // security token back so we can then use it to sign the user to SPO 

                SamlSecurityToken samlST = new SamlSecurityToken();
                byte[] saml11RTBytes = null;
                string logonToken = null;

                // find out whether the user's domain is a federated domain
                this.adfsAuthUrl = GetAdfsAuthUrl();

                // get logon token using windows integrated auth when the user is connected to the corporate network 
                if (this.adfsAuthUrl != null && this.useIntegratedWindowsAuth)
                {
                    UriBuilder ub = new UriBuilder();
                    ub.Scheme = this.adfsAuthUrl.Scheme;
                    ub.Host = this.adfsAuthUrl.Host;
                    ub.Path = string.Format("{0}auth/integrated/", this.adfsAuthUrl.LocalPath);

                    // specify in the query string we want a logon token to present to the Microsoft Federation Gateway
                    // for the corresponding user
                    ub.Query = String.Format("{0}&wa=wsignin1.0&wtrealm=urn:federation:MicrosoftOnline", this.adfsAuthUrl.Query.Remove(0, 1)).
                        Replace("&username=", String.Format("&username={0}", this.username));

                    this.adfsIntegratedAuthUrl = ub.Uri;

                    // get the logon token from the corporate ADFS using Windows Integrated Auth
                    logonToken = GetAdfsSAMLTokenWinAuth();

                    if (!string.IsNullOrEmpty(logonToken))
                    {
                        // generate the WS-Trust security token request SOAP message passing in the logon token we got from the corporate ADFS
                        // and the site we want access to 
                        saml11RTBytes = Encoding.UTF8.GetBytes(ParameterizeSoapRequestTokenMsgWithAssertion(
                            this.spSiteUrl.ToString(),
                            logonToken,
                            msoStsUrl));
                    }
                }

                // get logon token using the user's corporate credentials. Likely when not connected to the corporate network
                if (logonToken == null && this.adfsAuthUrl != null && !string.IsNullOrEmpty(password))
                {
                    logonToken = GetAdfsSAMLTokenUsernamePassword(); // get the logon token from the corporate ADFS proxy usernamemixed enpoint

                    if (logonToken != null)
                    {
                        // generate the WS-Trust security token request SOAP message passing in the logon token we got from the corporate ADFS
                        // and the site we want access to 
                        saml11RTBytes = Encoding.UTF8.GetBytes(ParameterizeSoapRequestTokenMsgWithAssertion(
                          this.spSiteUrl.ToString(),
                          logonToken,
                          msoStsUrl));
                    }
                }

                if (logonToken == null && this.adfsAuthUrl == null && !string.IsNullOrEmpty(password)) // login with O365 credentials. Not a federated login.
                {
                    // generate the WS-Trust security token request SOAP message passing in the user's credentials and the site we want access to 
                    saml11RTBytes = Encoding.UTF8.GetBytes(ParameterizeSoapRequestTokenMsgWithUsernamePassword(
                        this.spSiteUrl.ToString(),
                        this.username,
                        this.password,
                        msoStsUrl));
                }

                if (saml11RTBytes != null)
                {
                    Uri MsoSTSUri = new Uri(msoStsUrl);

                    HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(MsoSTSUri);


                    byte[] responseData = HttpHelper.SendHttpRequest(
                        MsoSTSUri,
                        "POST",
                        saml11RTBytes,
                        "application/soap+xml; charset=utf-8",
                        request,
                        null);

                    StreamReader sr = new StreamReader(new MemoryStream(responseData), Encoding.GetEncoding("utf-8"));
                    XPathNavigator nav = new XPathDocument(sr).CreateNavigator();
                    XmlNamespaceManager nsMgr = new XmlNamespaceManager(nav.NameTable);
                    nsMgr.AddNamespace("wsse", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
                    XPathNavigator binarySecurityToken = nav.SelectSingleNode("//wsse:BinarySecurityToken", nsMgr);

                    if (binarySecurityToken != null)
                    {
                        string binaryST = binarySecurityToken.InnerXml;

                        nsMgr.AddNamespace("wsu", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
                        XPathNavigator expires = nav.SelectSingleNode("//wsu:Expires", nsMgr);

                        if (!String.IsNullOrEmpty(binarySecurityToken.InnerXml) && !String.IsNullOrEmpty(expires.InnerXml))
                        {
                            samlST.Token = Encoding.UTF8.GetBytes(binarySecurityToken.InnerXml);
                            samlST.Expires = DateTime.Parse(expires.InnerXml);
                        }
                    }
                    else
                    {
                        // We didn't get security token
                    }
                }

                return samlST;
            }

            private string ParameterizeSoapRequestTokenMsgWithUsernamePassword(string url, string username, string password, string toUrl)
            {
                System.Text.StringBuilder s = new System.Text.StringBuilder();
                s.Append("<s:Envelope xmlns:s=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:a=\"http://www.w3.org/2005/08/addressing\" xmlns:u=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\">");
                s.Append("<s:Header>");
                s.Append("<a:Action s:mustUnderstand=\"1\">http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Issue</a:Action>");
                s.Append("<a:ReplyTo>");
                s.Append("<a:Address>http://www.w3.org/2005/08/addressing/anonymous</a:Address>");
                s.Append("</a:ReplyTo>");
                s.Append("<a:To s:mustUnderstand=\"1\">[toUrl]</a:To>");
                s.Append("<o:Security s:mustUnderstand=\"1\" xmlns:o=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\">");
                s.Append("<o:UsernameToken>");
                s.Append("<o:Username>[username]</o:Username>");
                s.Append("<o:Password>[password]</o:Password>");
                s.Append("</o:UsernameToken>");
                s.Append("</o:Security>");
                s.Append("</s:Header>");
                s.Append("<s:Body>");
                s.Append("<t:RequestSecurityToken xmlns:t=\"http://schemas.xmlsoap.org/ws/2005/02/trust\">");
                s.Append("<wsp:AppliesTo xmlns:wsp=\"http://schemas.xmlsoap.org/ws/2004/09/policy\">");
                s.Append("<a:EndpointReference>");
                s.Append("<a:Address>[url]</a:Address>");
                s.Append("</a:EndpointReference>");
                s.Append("</wsp:AppliesTo>");
                s.Append("<t:KeyType>http://schemas.xmlsoap.org/ws/2005/05/identity/NoProofKey</t:KeyType>");
                s.Append("<t:RequestType>http://schemas.xmlsoap.org/ws/2005/02/trust/Issue</t:RequestType>");
                s.Append("<t:TokenType>urn:oasis:names:tc:SAML:1.0:assertion</t:TokenType>");
                s.Append("</t:RequestSecurityToken>");
                s.Append("</s:Body>");
                s.Append("</s:Envelope>");


                string samlRTString = s.ToString();
                samlRTString = samlRTString.Replace("[username]", username);
                samlRTString = samlRTString.Replace("[password]", password);
                samlRTString = samlRTString.Replace("[url]", url);
                samlRTString = samlRTString.Replace("[toUrl]", toUrl);

                return samlRTString;
            }

            private string ParameterizeSoapRequestTokenMsgWithAssertion(string url, string samlAssertion, string toUrl)
            {
                System.Text.StringBuilder s = new System.Text.StringBuilder();
                s.Append("<s:Envelope xmlns:s=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:a=\"http://www.w3.org/2005/08/addressing\" xmlns:u=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\">");
                s.Append("<s:Header>");
                s.Append("<a:Action s:mustUnderstand=\"1\">http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Issue</a:Action>");
                s.Append("<a:ReplyTo>");
                s.Append("<a:Address>http://www.w3.org/2005/08/addressing/anonymous</a:Address>");
                s.Append("</a:ReplyTo>");
                s.Append("<a:To s:mustUnderstand=\"1\">[toUrl]</a:To>");
                s.Append("<o:Security s:mustUnderstand=\"1\" xmlns:o=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\">[assertion]");
                s.Append("</o:Security>");
                s.Append("</s:Header>");
                s.Append("<s:Body>");
                s.Append("<t:RequestSecurityToken xmlns:t=\"http://schemas.xmlsoap.org/ws/2005/02/trust\">");
                s.Append("<wsp:AppliesTo xmlns:wsp=\"http://schemas.xmlsoap.org/ws/2004/09/policy\">");
                s.Append("<a:EndpointReference>");
                s.Append("<a:Address>[url]</a:Address>");
                s.Append("</a:EndpointReference>");
                s.Append("</wsp:AppliesTo>");
                s.Append("<t:KeyType>http://schemas.xmlsoap.org/ws/2005/05/identity/NoProofKey</t:KeyType>");
                s.Append("<t:RequestType>http://schemas.xmlsoap.org/ws/2005/02/trust/Issue</t:RequestType>");
                s.Append("<t:TokenType>urn:oasis:names:tc:SAML:1.0:assertion</t:TokenType>");
                s.Append("</t:RequestSecurityToken>");
                s.Append("</s:Body>");
                s.Append("</s:Envelope>");

                string samlRTString = s.ToString();
                samlRTString = samlRTString.Replace("[assertion]", samlAssertion);
                samlRTString = samlRTString.Replace("[url]", url);
                samlRTString = samlRTString.Replace("[toUrl]", toUrl);

                return samlRTString;
            }

        }


    }
}
