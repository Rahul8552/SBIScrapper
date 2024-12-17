using System.Collections.ObjectModel;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using RestSharp;
using SeleniumExtras.WaitHelpers;

namespace IOBBankScrapper
{
    internal abstract class Program
    {
        private static string? UserId; 
        private static string? Password;
        private static string? LoginId;
        private static string? Upiid;

        // private const string BankUrl = "https://bankofindia.co.in/";
        private const string BankUrl = "https://starconnectcbs.bankofindia.com/BankAwayRetail/sgonHttpHandler.aspx?Action.RetUser.Init.001=Y&AppSignonBankId=013&AppType=retail";
        private static readonly string UpiIdStatusURL = "https://91.playludo.app/api/CommonAPI/GetUpiStatus?upiId=" + Upiid;
        private const string SaveTransactionUrl = "https://91uat.playludo.app/api/CommonAPI/SavebankTransaction";
        private static readonly string UpiIdUpdateUrl = "https://91.playludo.app/api/CommonAPI/UpdateDateBasedOnUpi?upiId=";
        private static readonly string GetCaptchaUrl = "https://91uat.playludo.app//api/captchareader";
        private static readonly string GetBeneficiaryDetailsURL = "https://91.playludo.app/api/CommonAPI/GetBeneficiaryDetailsForAutoTransfer?UserName=";
        private static readonly string GetCaptchaMethodTypeURL = "https://91.playludo.app/api/CommonAPI/GetCaptchSolveType?UserName=";

        public static void Main(string[] args)
        {
            getConfig();
            startAgain:
            // if (GetBankStatusViaUPIId() != "1")
            // {
            //     log("UPI ID is not active");
            //     sleep(10);
            //     goto startAgain;
            // } 

            var driver = InitBrowser();
            try
            {
                while (true)
                {
                    var transactionList = GetTransaction(driver);
                    log("Going to Save Transaction's");
                    SaveTransaction(transactionList);
                }
            }
            catch (Exception Ex)
            {
                log(Ex.Message);
                LogOutButton(driver);
                driver.Quit();
                sleep(10);
                goto startAgain;
            }
        }

        private static IWebDriver InitBrowser()
        {
            log("Initializing browser...");
            var chromeOptions = new ChromeOptions
            {
                PageLoadStrategy = PageLoadStrategy.Eager
            };

            string currentDirectory = Directory.GetCurrentDirectory();
            log("Current directory: " + currentDirectory);

            chromeOptions.AddArgument("--disable-blink-features");
            chromeOptions.AddArgument("--disable-blink-features=AutomationControlled");
            chromeOptions.AddArgument("--log-level=3");
            chromeOptions.AddArgument("--incognito");

            IWebDriver driver = new ChromeDriver(chromeOptions);
            log("Browser initialized.");
            driver.Manage().Window.Maximize();
            try
            {
                string ReturnStatus = Login(driver); // 1 for success, 2 for failure
                if (ReturnStatus == "1")
                {
                    log("Login Successful");
                    return driver;
                }

                log("Failed to login...");
                log("Closing browser...");
                driver.Quit();
            }
            catch (Exception Ex)
            {
                log("Failed to login...");
                log(Ex.Message);
                log("Closing browser...");
                driver.Quit();
            }

            return driver;
        }

        // private static string Login(IWebDriver driver)
        // {
        //     var solvedCaptcha = "";
        //     try
        //     {
        //         log("Logging in...");
        //         driver.Navigate().GoToUrl(BankUrl);
        //         WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
        //         
        //         bool Islogin = true;
        //         while (Islogin)
        //         {
        //             log("Click Internet Banking");
        //             wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("/html/body/div[1]/header/div/div[3]/div/div/nav/div/ul/li/div/section/div/div/div/div/ul/li[10]/a")));
        //             driver.FindElement(By.XPath("/html/body/div[1]/header/div/div[3]/div/div/nav/div/ul/li/div/section/div/div/div/div/ul/li[10]/a")).Click();
        //             
        //             log("Select Personal Login");
        //             wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("/html/body/div[1]/header/div/div[3]/div/div/nav/div/ul/li/div/section/div/div/div/div/ul/li[10]/div/div[3]/span[2]/a")));
        //             driver.FindElement(By.XPath("/html/body/div[1]/header/div/div[3]/div/div/nav/div/ul/li/div/section/div/div/div/div/ul/li[10]/div/div[3]/span[2]/a")).Click();
        //             
        //             log("Agree Terms and Conditions");
        //             wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("/html/body/div[1]/div[1]/div/footer/div/section/div/div[2]/div/div/div/div/div/div/div[3]/button")));
        //             driver.FindElement(By.XPath("/html/body/div[1]/div[1]/div/footer/div/section/div/div[2]/div/div/div/div/div/div/div[3]/button")).Click();
        //             
        //             TypingElement(driver, 30, By.Id("CorporateSignonCorpId"), LoginId, "LoginId");
        //             TypingElement(driver, 30, By.Id("id_password"), Password, "Password");
        //             
        //             log("Trying solve captch");
        //             sleep(2);
        //             wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("/html/body/jsp:forward/form/center/div/div/div[2]/div[3]/div[1]/img[2]")));
        //
        //             IWebElement imageElement = driver.FindElement(By.XPath("/html/body/jsp:forward/form/center/div/div/div[2]/div[3]/div[1]/img[2]"));
        //             
        //             var imagecaptcha = driver.FindElement(By.XPath("/html/body/jsp:forward/form/center/div/div/div[2]/div[3]/div[1]/img[2]"));
        //             var elementScreenshot = ((ITakesScreenshot)imagecaptcha).GetScreenshot();
        //             
        //             const string fileName = "captchaFile";
        //             var isSaved = SaveImage(elementScreenshot.AsBase64EncodedString, fileName);
        //             if (!isSaved)
        //             {
        //                 return "2";
        //             }
        //
        //             solvedCaptcha = CaptchaCodeSolve().ToUpper();
        //             bool isReloved = false;
        //             if (solvedCaptcha != "1")
        //             {
        //                 log("Captcha code is " + solvedCaptcha);
        //                 wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("/html/body/jsp:forward/form/center/div/div/div[2]/div[3]/div[1]/input[3]")));
        //                 TypingElement(driver, 10, By.XPath("/html/body/jsp:forward/form/center/div/div/div[2]/div[3]/div[1]/input[3]"), solvedCaptcha, "Captcha");
        //                 
        //                 wait.Until(ExpectedConditions.ElementIsVisible(By.Id("button1")));
        //                 driver.FindElement(By.Id("button1")).Click();
        //
        //                 isReloved = !CheckCaptchResolved(driver);
        //                 if (isReloved)
        //                     Islogin = false;
        //             }
        //             else
        //             {
        //                 driver.FindElement(By.XPath(
        //                         "/html/body/div/div/app-root/app-prelogin-component/div/div/div/div/app-login/div[1]/div/div[2]/form/div/div[6]/app-image-audio-captcha/form/div[1]/div/div[2]/div/div/a"))
        //                     .Click();
        //             }
        //             
        //             // if (solvedCaptcha == "1")
        //             //     return "2";
        //             //
        //             // log("Captcha code is " + solvedCaptcha);
        //             // solvedCaptcha = solvedCaptcha.Length > 5 ? solvedCaptcha.Trim().Replace(" ", "") : "123456";
        //             //
        //             // TypingElement(driver, 10, By.XPath("/html/body/form/div[2]/div[2]/div[1]/div/div[5]/input"), solvedCaptcha, "Captcha");
        //             //
        //             // wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("/html/body/form/div[2]/div[2]/div[1]/div/button[1]")));
        //             // driver.FindElement(By.XPath("/html/body/form/div[2]/div[2]/div[1]/div/button[1]")).Click();
        //             //
        //             // sleep(2);
        //             // bool isReloved = CheckCaptchResolved(driver);
        //             // if (!isReloved)
        //             // {
        //             //     Islogin = false;
        //             // }
        //             // else
        //             // {
        //             //     log("Captcha not resolved. Retrying...");
        //             // }
        //         }
        //     }
        //     catch (Exception e)
        //     {
        //         Console.WriteLine(e.Message);
        //         return "2";
        //     }
        //
        //     return "1";
        // }
        
        private static string Login(IWebDriver driver)
        {
            var solvedCaptcha = "";
            const int maxCaptchaAttempts = 2; // Maximum CAPTCHA solving attempts before restarting
            int captchaFailures = 0;

            try
            {
                log("Logging in...");
                driver.Navigate().GoToUrl(BankUrl);
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                
                bool isLogin = true;
                while (isLogin)
                {
                    // log("Close Popup");
                    // wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("/html/body/div[1]/div[1]/div/section/div[1]/div[2]/div/div/section/div/div[2]/div/div/div/div/div/div/div/div[1]/button/picture/img")));
                    // driver.FindElement(By.XPath("/html/body/div[1]/div[1]/div/section/div[1]/div[2]/div/div/section/div/div[2]/div/div/div/div/div/div/div/div[1]/button/picture/img")).Click();
                    //
                    // log("Click Internet Banking");
                    // wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("/html/body/div[1]/header/div/div[3]/div/div/nav/div/ul/li/div/section/div/div/div/div/ul/li[10]/a")));
                    // driver.FindElement(By.XPath("/html/body/div[1]/header/div/div[3]/div/div/nav/div/ul/li/div/section/div/div/div/div/ul/li[10]/a")).Click();
                    //
                    // log("Select Personal Login");
                    // wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("/html/body/div[1]/header/div/div[3]/div/div/nav/div/ul/li/div/section/div/div/div/div/ul/li[10]/div/div[3]/span[2]/a")));
                    // driver.FindElement(By.XPath("/html/body/div[1]/header/div/div[3]/div/div/nav/div/ul/li/div/section/div/div/div/div/ul/li[10]/div/div[3]/span[2]/a")).Click();
                    //
                    // log("Agree Terms and Conditions");
                    // wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("/html/body/div[1]/div[1]/div/footer/div/section/div/div[2]/div/div/div/div/div/div/div[3]/button")));
                    // driver.FindElement(By.XPath("/html/body/div[1]/div[1]/div/footer/div/section/div/div[2]/div/div/div/div/div/div/div[3]/button")).Click();
                    
                    wait.Until(ExpectedConditions.ElementIsVisible(By.Id("CorporateSignonCorpId")));
                    //IWebElement elementInsideIframe = TypingElement(driver, 30, By.Id("/html/body/jsp:forward/form/center/div/div/div[2]/div[3]/div[1]/input[1]"), UserId, "UserId")
                    TypingElement(driver, 30, By.Id("CorporateSignonCorpId"), UserId, "UserId");
                    TypingElement(driver, 30, By.Id("id_password"), Password, "Password");
                    
                    log("Trying to solve CAPTCHA");
                    sleep(2);
                    string simpleXpath = "/html/body//form/center/div/div/div[2]/div[3]/div[1]/img[2]";
                    wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(simpleXpath)));
                    

                    IWebElement imageElement = driver.FindElement(By.XPath(simpleXpath));
                    
                    var imageCaptcha = driver.FindElement(By.XPath(simpleXpath));
                    var elementScreenshot = ((ITakesScreenshot)imageCaptcha).GetScreenshot();
                    
                    const string fileName = "captchaFile";
                    var isSaved = SaveImage(elementScreenshot.AsBase64EncodedString, fileName);
                    if (!isSaved)
                    {
                        return "2";
                    }

                    solvedCaptcha = CaptchaCodeSolve().ToUpper();
                    
                    if (solvedCaptcha != "1")
                    {
                        log("Captcha code is " + solvedCaptcha);
                        wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("/html/body/jsp:forward/form/center/div/div/div[2]/div[3]/div[1]/input[3]")));
                        TypingElement(driver, 10, By.XPath("/html/body/jsp:forward/form/center/div/div/div[2]/div[3]/div[1]/input[3]"), solvedCaptcha, "Captcha");
                        
                        wait.Until(ExpectedConditions.ElementIsVisible(By.Id("button1")));
                        driver.FindElement(By.Id("button1")).Click();

                        if (CheckCaptchResolved(driver))
                        {
                            log("Captcha not resolved. Retrying...");
                            captchaFailures++;

                            if (captchaFailures >= maxCaptchaAttempts)
                            {
                                log("Max CAPTCHA attempts reached. Restarting login process...");
                                captchaFailures = 0; // Reset failure counter
                                driver.Navigate().GoToUrl(BankUrl); // Restart from the beginning
                                continue;
                            }
                        }
                        else
                        {
                            isLogin = false; // Login successful
                        }
                    }
                    else
                    {
                        log("Captcha solve failed. Clicking refresh...");
                        driver.FindElement(By.XPath(
                                "/html/body/div/div/app-root/app-prelogin-component/div/div/div/div/app-login/div[1]/div/div[2]/form/div/div[6]/app-image-audio-captcha/form/div[1]/div/div[2]/div/div/a"))
                            .Click();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return "2";
            }

            return "1";
        }

        
        private static object GetTransaction(IWebDriver driver)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(40));
            sleep(2);
            
            List<object> list = new List<object>();
            if (list == null) throw new ArgumentNullException(nameof(list));
            // if (GetBankStatusViaUPIId() != "1")
            // {
            //     log("Bank is not active");
            //     sleep(3);
            //     return list;
            // }

            //// To get Available balance
            sleep(2);
            // string avlBal = GetAvailBalace(driver);
            
            string AvlBal = GetAvailBalace(driver);

            // To get Transaction data
            sleep(2);
            log("Clicked last few transactions...");
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("/html/body/div[1]/div[1]/div[2]/div[2]/div[1]/div[1]/nav/ul/li[2]/a")));
            driver.FindElement(By.XPath("/html/body/div[1]/div[1]/div[2]/div[2]/div[1]/div[1]/nav/ul/li[2]/a")).Click();

            log("Clicked on Account Number...");
            wait.Until(ExpectedConditions.ElementIsVisible(
                By.XPath("/html/body/div[1]/div[1]/div[2]/div[2]/div[2]/form/div/div/div/div/div/div[1]/div/div/div/table/tbody/tr/td[2]/a")));
            driver.FindElement(
                By.XPath("/html/body/div[1]/div[1]/div[2]/div[2]/div[2]/form/div/div/div/div/div/div[1]/div/div/div/table/tbody/tr/td[2]/a")).Click();

            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("/html/body/div[3]/div[2]/table/tbody/tr[1]")));
            log("Row visible...");

            IWebElement tbl1 = driver.FindElement(By.XPath("/html/body/div[3]/div[2]/table/tbody"));
            ReadOnlyCollection<IWebElement> TableRow = tbl1.FindElements(By.TagName("tr"));
            log("Row found...");

            int i = TableRow.Count;
            for (int j = 0; j < i; j++)
            {
                var row = TableRow[j];
                var cols = row.FindElements(By.TagName("td"));
                if (cols.Count <= 0) continue;
                string CreatedDate = cols[0].Text;
                string Description = cols[2].Text + cols[1].Text;
                string Amount = "";
                Description = GetTheUTRWithoutUTR(Description);
                
                if (cols[2].Text == "Debit")
                {
                    Amount = "-" + cols[3].Text.Replace(" ", "");
                }
                else
                {
                    Amount = cols[3].Text.Replace(" ", "");
                }

                Amount = Amount.Replace(",", "");
                AvlBal = AvlBal.Replace(",", "");
                string RefNumber = Description;
                string AccountBalance = AvlBal;
                string UPIId = GetUPIId(Description);

                var values = new
                {
                    Description,
                    CreatedDate,
                    Amount,
                    RefNumber,
                    AccountBalance,
                    BankName = "IOB - " + UserId,
                    UPIId,
                    BankLoginId = UserId
                };
                list.Add(values);
            }

            log("Going back to transaction list...");
            driver.FindElement(By.XPath("/html/body/div[3]/div[1]/button")).Click();
            string json = JsonConvert.SerializeObject(list);
            log(json);
            return list;
        }
        
        private static void SaveTransaction(object TransactionList)
        {
            log("Saving the transaction...");
            string json = JsonConvert.SerializeObject(TransactionList);

            var options = new RestClientOptions(SaveTransactionUrl)
            {
                MaxTimeout = -1,
            };
            var client = new RestClient(options);
            var request = new RestRequest("", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", json, ParameterType.RequestBody);
            RestResponse response = client.Execute<RestResponse>(request);
            log(response.Content ?? "");
            UpdateUPIDate();
        }
        
        private static string GetAvailBalace(IWebDriver driver)
        {
            int i = 0;
            while (i < 4)
            {
                try
                {
                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(50));

                    log("Clicked Balance Enquiry...");
                    wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("/html/body/div/div[1]/div[2]/div[2]/div[1]/div[1]/nav/ul/li[1]")));
                    driver.FindElement(By.XPath("/html/body/div/div[1]/div[2]/div[2]/div[1]/div[1]/nav/ul/li[1]")).Click();
                    
                    sleep(1);
                    log("Clicked Account Number to get Avail Bal...");
                    wait.Until(ExpectedConditions.ElementIsVisible(
                        By.XPath("/html/body/div/div[1]/div[2]/div[2]/div[2]/form/div/div/div/div/div/div[1]/div/div/div/table/tbody/tr/td[2]/a")));
                    driver.FindElement(
                            By.XPath("/html/body/div/div[1]/div[2]/div[2]/div[2]/form/div/div/div/div/div/div[1]/div/div/div/table/tbody/tr/td[2]/a")).Click();

                    log("Checked Available Balance...");
                    wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("/html/body/div[3]/div[2]/table/tbody/tr/td[1]")));
                    string AvlBal = driver.FindElement(By.XPath("/html/body/div[3]/div[2]/table/tbody/tr/td[1]")).Text;

                    wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("/html/body/div[3]/div[1]/button")));
                    driver.FindElement(By.XPath("/html/body/div[3]/div[1]/button")).Click();
                    return AvlBal;
                }
                catch (Exception e)
                {
                    log("Exception in GetAvailBalace");
                    driver.FindElement(By.XPath("/html/body/div[3]/div[1]/button")).Click();
                }

                i++;
            }

            return "0";
        }
        
        private static bool SaveImage(string ImgStr, string ImgName)
        {
            if (!Directory.Exists("ScreenShotFolder"))
            {
                Directory.CreateDirectory("ScreenShotFolder");
            }

            string imageName = ImgName + ".jpg";
            string imgPath = Path.Combine("ScreenShotFolder", imageName);
            byte[] imageBytes = Convert.FromBase64String(ImgStr);
            File.WriteAllBytes(imgPath, imageBytes);
            
            return true;
        }
        
        private static string CaptchaCodeSolve()
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo("ScreenShotFolder/");
                string newImageDir = di.FullName;
                var options = new RestClientOptions(GetCaptchaUrl)
                {
                    MaxTimeout = -1,
                };
                var client = new RestClient(options);
                var request = new RestRequest("", RestSharp.Method.Post);
                request.AddFile("image", newImageDir + "captchaFile.jpg");
                RestResponse response = client.Execute<RestResponse>(request);
                var responseData = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Content ?? "");
                if (responseData["ErrorCode"] == "1")
                    return responseData["ErrorMessage"];
                else
                    return "1";
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "1";
            }
        }
        
        private static string GetCaptchaCodeFromAZCaptch(string ImgStr)
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, "http://azcaptcha.com/in.php");
                var content = new MultipartFormDataContent();
                content.Add(new StringContent(ImgStr), "body");
                content.Add(new StringContent("2tjddcrmhqnwpy9km3l4xrgqvb8xyb7v"), "key");
                content.Add(new StringContent("base64"), "method");
                content.Add(new StringContent("1"), "json");
                request.Content = content;
                var responseTask = client.SendAsync(request);
                responseTask.Wait();
                var response = responseTask.Result;
                response.EnsureSuccessStatusCode();
                var responseContentTask = response.Content.ReadAsStringAsync(); 
                responseContentTask.Wait();
                var responseContent = responseContentTask.Result;
                var responseObject = JsonConvert.DeserializeObject<ResponseObject>(responseContent);


                if (responseObject != null)
                {
                    var getCaptcha = GetSolvedCaptchString(responseObject.RequestId);
;                    return getCaptcha;
                }
                else
                {
                    return "";   
                }
            }
            catch (Exception ex)
            {
                log("exception1 " + ex.Message);
                return "1";
            }
        }

        private static string GetSolvedCaptchString(string requestId)
        {
            try
            {
                var options = new RestClientOptions("http://azcaptcha.com")
                {
                    MaxTimeout = -1,
                };
                var client = new RestClient(options);
                var request = new RestRequest("/res.php?key=2tjddcrmhqnwpy9km3l4xrgqvb8xyb7v&id="+ requestId +"&action=get", Method.Get);
                sleep(10);
                RestResponse response = client.Execute<RestResponse>(request);
                
                string responseData = response.Content;
                string[] parts = responseData.Split('|');
                string solvedCaptcha = UppercaseCaptcha(parts[1]);
                return solvedCaptcha;
            }
            catch (Exception ex)
            {
                log("exception1 " + ex.Message);
                return "1";
            }
        }

        private static string UppercaseCaptcha(string captcha)
        {
            try
            {
                string result = "";
                foreach (char c in captcha)
                {
                    if (Char.IsLetter(c))
                    {
                        result += Char.ToUpper(c);
                    }
                    else
                    {
                        result += c;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                log("exception1 " + ex.Message);
                return "1";
            }
        }

        private static bool CheckCaptchResolved(OpenQA.Selenium.IWebDriver driver)
        {
            try
            {
                string currenturl = driver.Url;
                if (currenturl.Contains("Incorrect"))
                {
                    log("Captcha Error");
                    return true;
                }

                log("Captcha solved");
                return false;
            }
            catch (Exception e)
            {
                return true;
            }
        }
        
        private static string GetBankStatusViaUPIId()
        {
            try
            {
                var options = new RestClientOptions(UpiIdStatusURL + Upiid)
                {
                    MaxTimeout = -1,
                };
                var client = new RestClient(options);
                var request = new RestRequest("", Method.Get);
                RestResponse response = client.Execute<RestResponse>(request);
                var responseData = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Content ?? "");
                return responseData != null ? responseData["Result"] : "2";
            }
            catch
            {
                return "2";
            }
        }

        private static void TypingElement(IWebDriver driver, int waitingtimeinsecond, By Selection, string? InputMessage, string message)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitingtimeinsecond));
            log("Waiting for " + message + " field...");
            wait.Until(ExpectedConditions.ElementIsVisible(Selection));
            log("Typing " + message + "...");

            driver.FindElement(Selection).Clear();
            driver.FindElement(Selection).SendKeys(InputMessage);
        }
        private static void UpdateUPIDate()
        {
            try
            {
                var options = new RestClientOptions(UpiIdUpdateUrl + Upiid)
                {
                    MaxTimeout = -1,
                };
                var client = new RestClient(options);
                var request = new RestRequest("", Method.Get);
                request.AddHeader("Content-Type", "application/json");
                RestResponse response = client.Execute<RestResponse>(request);
                log(response.Content ?? "");
            }
            catch (Exception e)
            {
                log(e.Message);
            }
        }

        private static string GetTheUTRWithoutUTR(string description)
        {
            try
            {
                if (description.Contains("NEFT"))
                {
                    var split = description.Split('-');
                    var value = split.FirstOrDefault(x => x.Length == 16);
                    
                    if (value != null)
                    {
                        if (description.Contains("UPI"))
                            return value + " " + description;
                        else
                            return value + " UPI " + description;
                    }
                    return description;
                }
                else if (description.Contains("RTGS"))
                {
                    var value = description.Substring(5, 22);
                    if (value != null)
                    {
                        if (description.Contains("UPI"))
                            return value + " " + description;
                        else
                            return value + " UPI " + description;
                    }
                    return description;
                }
                else
                {
                        var split = description.Split('/');
                        var value = split.FirstOrDefault(x => x.Length == 12);
                        if (value != null)
                        {
                            if (description.Contains("UPI"))
                                return value + " " + description;
                            else
                                return value + " UPI " + description;
                        }
                        return description;
                }
            }
            catch
            {
                return description;
            }
            return description;
        }

        private static string GetUPIId(string description)
        {
            try
            {
                if (!description.Contains("@")) return "";
                var split = description.Split('/');
                var value = split.FirstOrDefault(x => x.Contains("@"));
                if (value != null)
                {
                    value = value.Replace("From:", "");
                }
                return value;
            }
            catch (Exception ex)
            {
                log(ex.Message);
            }
            return "";
        }
        
        private static string GetCaptchaMethodType()
        {
            try
            {
                var options = new RestClientOptions(GetCaptchaMethodTypeURL + UserId +"&UpiId="+ Upiid)
                {
                    MaxTimeout = -1,
                };

                var client = new RestClient(options);
                var request = new RestRequest("", Method.Get);
                RestResponse response = client.Execute<RestResponse>(request);

                var responseData = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Content ?? "");
                return responseData != null ? responseData["Result"] : "";
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "1";
            }
        }
        
        private static string GetOTPFromAPI(DateTime newDateTime)
        {
            try
            {
                int RetryCount = 0;
                while (RetryCount < 10)
                {
                    sleep(5);
                    log("Trying to get OTP from API.. Attempt " + RetryCount);
                    string OTPAPI = getOTPRequest();
                    if (OTPAPI != "")
                    {
                        var responseData = JsonConvert.DeserializeObject<Dictionary<string, string>>(OTPAPI);
                        log(newDateTime.ToString());
                        if (responseData["ErrorMessage"] != "" && DateTime.Parse(responseData["ErrorMessage"]) > newDateTime)
                        {
                            if (responseData["ErrorCode"] == "1")
                            {
                                log("OTP received from API " + responseData["Result"]);

                                return responseData["Result"];
                            }
                        }
                        log("Failed to get OTP from API");
                    }
                    RetryCount++;
                }
            }
            catch (Exception e)
            {
                log("Failed to get OTP from API");
            }
            return "1";
        }
        
        private static string getOTPRequest()
        {
            try
            {
                string OTPurl = "https://91.playludo.app/api/CommonAPI/GetBankPhoneOTPViaUPIId?UpiId=" + UserId;
                log("Getting OTP...");
                var options = new RestClientOptions(OTPurl)
                {
                    MaxTimeout = -1
                };
                var client = new RestClient(options);
                var request = new RestRequest("", Method.Get);
                request.AddHeader("Content-Type", "application/json");
                RestResponse response = client.Execute<RestResponse>(request);
                log(response.Content);
                return response.Content;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return "";
        }
        
        static bool IsAlertPresent(IWebDriver driver)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                wait.Until(ExpectedConditions.AlertIsPresent());
                return true;
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        static void AlertHandle(IWebDriver driver)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            wait.Until(ExpectedConditions.AlertIsPresent());
            IAlert alert = driver.SwitchTo().Alert();
            string alertText = alert.Text;
            Console.WriteLine("Alert text: " + alertText);
            alert.Accept();
        }
        
        private static void getConfig()
        {
            var configurationBuilder = new ConfigurationBuilder();

            configurationBuilder.AddJsonFile("appsettings.json");
            var configuration = configurationBuilder.Build();
            var appSettingsSection = configuration.GetSection("AppSettings");
            Password = appSettingsSection["Password"];
            Upiid = appSettingsSection["Upiid"];
            UserId = appSettingsSection["UserName"];
        }

        private static void LogOutButton(IWebDriver driver)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("/html/body/div/div[1]/div[1]/div/div/div/div/div/div/div/a")));
                driver.FindElement(By.XPath("/html/body/div/div[1]/div[1]/div/div/div/div/div/div/div/a")).Click();
                sleep(2);
                driver.Quit();
                log("Logged Out.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        
        private static void log(string Message)
        {
            string PrintMessage = "[" + DateTime.Now.ToString("MMMM dd, yyyy h:mm:ss tt") + " - " + Upiid + "] " + Message;
            Console.WriteLine(PrintMessage);
        }
        private static void sleep(int Second)
        {
            log("Sleeping for " + Second.ToString() + " Seconds....");
            Thread.Sleep(Second * 1000);
        }
        public class ResponseObject
        {
            [JsonProperty("status")]
            public int Status { get; set; }

            [JsonProperty("request")]
            public string RequestId { get; set; }
        }
    }
}