**Preparation for AI-100**

# Evaluate text with Azure Cognitive Language Services

## Classify and moderate text with Azure Content Moderator

### Overview of text moderation
- Machine-assisted content moderation
- Places like chat rooms, discussion boards, chatbots, ecommerce catalogs, documents
- Response from Text Moderation API :
	- A list of potentially unwanted words found in the text.
	- What type of potentially unwanted words were found.
	- Possible personally identifiable information (PII) found in the text.

#### Profanity
When you pass text to the API, any potentially profane terms in the text are identified and returned in a JSON response. The profane item is returned as a Term in the JSON response, along with an index value showing where the term is in the supplied text.

        "Terms": [
        {
            "Index": 118,
            "OriginalIndex": 118,
            "ListId": 0,
            "Term": "crap"
        }

#### Classification
This feature of the API can place text into specific categories based on the following specifications:
- Category 1: Potential presence of language that might be considered sexually explicit or adult in certain situations.
- Category 2: Potential presence of language that might be considered sexually suggestive or mature in certain situations.
- Category 3: Potential presence of language that might be considered offensive in certain situations.

        "Classification": {
            "ReviewRecommended": true,
            "Category1": {
                "Score": 1.5113095059859916E-06
                },
            "Category2": {
                "Score": 0.12747249007225037
                },
            "Category3": {
                "Score": 0.98799997568130493
            }
        }
        
- returns bool value for recommended review of the text (if true review content manually)
- Category returns a score between 0 and 1
- higher the score category might apply

#### Personally identifiable information
- critical importance in many applications
- detect if any values in the text might be considered PII before you release it publicly
- key aspects - Email addresses, US mailing addresses, IP addresses, US phone numbers, UK phone numbers, Social Security numbers
- If possible PII values are found below is how the response looks

        "PII": {
            "Email": [{
                "Detected": "abcdef@abcd.com",
                "SubType": "Regular",
                "Text": "abcdef@abcd.com",
                "Index": 32
                }],
            "IPA": [{
                "SubType": "IPV4",
                "Text": "255.255.255.255",
                "Index": 72
                }],
            "Phone": [{
                "CountryCode": "US",
                "Text": "5557789887",
                "Index": 56
                }, {
                "CountryCode": "UK",
                "Text": "+44 123 456 7890",
                "Index": 208
                }],
            "Address": [{
                "Text": "1 Microsoft Way, Redmond, WA 98052",
                "Index": 89
                }],
            "SSN": [{
                "Text": "999-99-9999",
                "Index": 267
                }]
            }
