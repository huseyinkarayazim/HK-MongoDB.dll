
# HK-MongoDB.dll

This DLL provides Object-Document Mapping (ODM) operations on MongoDB.
 ## This is not for profesional using! 



## Installation

Install my-project with git

```bash
  git clone https://github.com/huseyinkarayazim/HK-MongoDB.dll.git
```
And i use my own HK.Security.dll for Cryptography you have to change string cipher in https://github.com/huseyinkarayazim/HK.Security/blob/master/StringCipher.cs for more security than you can add referance.

Or you can use your own methods but you have to change all HK.Security.Encrypt / HK.Security.Decrypt  methods.

## Attension !

You have to re-configuration some datas.

1. Step : Go to /Enum/DBName.cs and add your database name.
2. Step : Go to /Enum/RequestSource.cs and add/change/delete your RequestSource if you want!
3. Step : Go to /MongoProcessor.cs and change this values:

        public static Enum.DBName DBName = Enum.DBName.HK_Database; // Your db name Here
        public static Enum.RequestSource RequestSource = Enum.RequestSource.Web; // You can change or delete.
4. Step : Go to /Collections/ file  and add your collection here.
5. Step : Go to /MongoManager.cs  if you want to add "MongoCredentials" you have to open and  change this comment line

        //private string Username { get { return "YourUserName"; } }

        //private string Password { get { return "YourPassword"; } }

        //private string AuthMechanism { get { return "SCRAM-SHA-256"; } }

        //private string AuthDbName { get { return "admin"; } }
and also open this method

        //public void SetMongoCredentials()
        //{
        //    MongoIdentity identity = new MongoInternalIdentity(AuthDbName, Username);
        //    MongoIdentityEvidence evidence = new PasswordEvidence(Password);
        //    setting.Credential = new MongoCredential(AuthMechanism, identity, evidence);
        //}
    
then search this method (SetMongoCredentials) in this class and open a comment because I added this method to all methods, but they are in the comment.




## Usage/Examples

```javascript
using HK_MongoDB.Collections;
using System;
namespace HK_MongoDB
{
    static class Program
    {
        static void Main()
        {
        // This method should be create your database and all collection!
           HK_MongoDB.Builders.BuildCollections(HK_MongoDB.Enum.RequestSource.Web);  
        // After that you can try this method Program.cs also contains these methods
            ExampleCollection exampleCollection = new ExampleCollection();
            exampleCollection.Id = MongoProcessor.GetObjectId();
            exampleCollection.ExampleField = "This Project created by Huseyin Karayazim!";
            if (MongoProcessor.Add<ExampleCollection>(exampleCollection))
        }
    }
}
```

## Contact

E-Mail : huseyinkarayazim@gmail.com
Web Site : [Visit My Website](https://www.huseyinkarayazim.com.tr/)
LinkedIn: [Hüseyin Karayazým](https://www.linkedin.com/in/h%C3%BCseyin-karayaz%C4%B1m-243290228/)

## License

HK-Mongo.dll is licensed under the [MIT License.](https://github.com/huseyinkarayazim/HK-MongoDB.dll/edit/master/LICENSE.txt)

