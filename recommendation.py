# coding: utf-8
import sys
import pandas as pd
import numpy as np
from sklearn.metrics.pairwise import cosine_similarity
from sklearn.feature_extraction.text import CountVectorizer
from pythainlp.corpus import thai_stopwords
from pythainlp import word_tokenize
import re

args = sys.argv

readFile = str(args[1])
active = int(args[2])
culture_name = str(args[3])
topN = int(args[4])

#________________________________________________________________________
def bagofwords(text):
    
    # 1. Remove special characters
    # 2. Make bag of words
    # 3. Remove  stopwords
    
    regexp_thai = re.compile(u"[^\u0E00-\u0E7Fa-zA-Z' ]|^'|'$|''") 
    #sign = (' ', ',', '.','..','...', ':', ';','%','+','-','*','/','ๆ','?','#','"',' ','\n',# เพิ่มเติม stop words 
    sign = (' ', ',', '.','..','...', ':', ';','%','+','-','*','/','ๆ','0','1','2','3','4','5','6','7','8','9','\n','\n\n','-','g','Kg','เสิร์ฟ','๐','๑','๒','๓','๔','๕','๖','๗','๘','๙',
            'ก','ข','ฃ','ค','ฅ','ง','จ','ฉ','ช','ญ','ณ','ด','ต','ถ','ท','ธ','น','บ','ป','ผ','ฝ','พ','ฟ','ภ','ม','ย','ร','ล','ว','ศ','ษ','ส','ห','อ',
            'ผสม','คั้น','หั่น','วัตถุดิบ','มล','Shallot','แง่ง','TIP','มล','มล.','Seasoning','เส','ริ์ฟ','วิธีทำ','เด',
            'ช้อนโต๊ะ','ช้อนชา','ถ้วย','ทัพพี','เล็กน้อย','หัว','กรัม','ml','cc','ต้น','ลูก','ตามชอบ','ห่อ','ชต','ชช','ซีก','ชิ้น','แท่ง','ลิตร','มิลลิลิตร','กิโลกรัม','ขีด','ฟอง',
           'แช่น้ำเย็น','พักไว้', 'สำหรับทอด','ดอก','Shredded')
    _STOPWORDS = thai_stopwords()
    _STOPSIGNS = frozenset(sign)
    ret = regexp_thai.sub("", text)
    bow0 = word_tokenize(ret, engine="newmm", keep_whitespace=False)
    bow1= [word for word in bow0 if( (word not in _STOPWORDS) and (word not in _STOPSIGNS))]
    return bow1


#________________________________________________________________________
#Make word set
def makeWordSet(df):
    wordSet = set([])
    for i in range(0,len(df.index)):
        #f[['foodname','ingredients','cul_name']]
        item1 = df.iloc[i]['name']
        #item2 = df.iloc[i]['ingredients']
        #item3 = df.iloc[i]['culture']
       
        bow = bagofwords(item1)
        #bow.append(item1)
        #bow.append(item3)
        wordSet = wordSet.union(set(bow))
    return wordSet
#________________________________________________________________________
#Make WordDict
def makeWordDictListandTFBowList(df,wordSet):
    #wordSet = makeWordSet(df)
    wordDict = dict.fromkeys(wordSet,0)
    wordDictList =[]
    tfBowList=[]
    for i in range(0, len(df.index)):
        wDict = dict.fromkeys(wordSet,0)
        #item = df.iloc[i]['CONTENT']
        item1 = df.iloc[i]['name']
        #item2 = df.iloc[i]['ingredients']
        #item3 = df.iloc[i]['culture']
        
        bow = bagofwords(item1)
        #bow.append(item1)
        #bow.append(item3)
        for word in bow:
            wDict[word]+=1
        tfBow = computeTF(wDict,bow)
        #print('i=',i)
        #print(bow)
        #print(wDict)
        wordDictList.append(wDict)
        tfBowList.append(tfBow)
        del wDict
        #del item1
        #del item2
        del item1
        del bow
    return (wordDictList, tfBowList)    
    
#________________________________________________________________________
# Collecting from various sites
# For finding TF
def computeTF(wordDict, bow):
    tfDict = {}
    bowCount = len(bow)
    for word, count in wordDict.items():
        tfDict[word] = count/float(bowCount)
    return tfDict
#_______________________________________________________________________
# For finding IDF
def computeIDF(docList):
    import math
    idfDict = {}
    N = len(docList)
    idfDict = dict.fromkeys(docList[0].keys(), 0)
    for doc in docList:
        for word, val in doc.items():
            if val > 0:
                idfDict[word] += 1   
    for word, val in idfDict.items():
        idfDict[word] = math.log10(N / float(val))
    return idfDict
#________________________________________________________________________
# For finding TF-IDF
def computeTFIDF(tfBow, idf):
    tfidf = {}
    for word, val in tfBow.items():
        tfidf[word] = val*idf[word]
    return tfidf
#________________________________________________________________________
# Creating TF-IDF List
def  createTFIDFMatrix(TFBowList,idf):
    tfidfMatrix=[]
    for tfBow in TFBowList:
        tfidfBow = computeTFIDF(tfBow,idf)
        tfidfMatrix.append(tfidfBow)
    return tfidfMatrix
#________________________________________________________________________
# Creating Similarity between all docs of the dataset
def MakeSim(df):
    #Input df of dataset
    sdf=df
    wordSet=makeWordSet(sdf)
    wordList = list(wordSet)
    wordDict = dict.fromkeys(wordSet,0)
    wordDictList, TFBowList = makeWordDictListandTFBowList(sdf, wordSet)
    #df_worddict
    df_worddict = pd.DataFrame(wordDictList)
    #create IDF
    idf = computeIDF(wordDictList)
    #create a matrix of TFIDF
    tfidfMatrix = createTFIDFMatrix(TFBowList,idf)
    #createTFIDFMatrix(TFBowList,idf):
    #convert it to dataframe
    pdMatrix   = pd.DataFrame(tfidfMatrix)
    #Calc for sim
    sim = cosine_similarity(pdMatrix, pdMatrix)
    #convert sim to df
    s=pd.DataFrame(sim)
    return tfidfMatrix, pdMatrix, sim, s
#________________________________________________________________________
#Making recommendations with top N
def RecFoodCulture(thedf, active, culture, s, topN):
    
    #retrieve only the active item
    a = s.iloc[active]
    #drop the active item similarity
    result = a.drop(active)
    #convert the result to list
    res=result.sort_values(ascending=False)
    resList = res.index.values.tolist()
    #active item data
    theactive = thedf.iloc[active]
     # assign top rec items 
    rec = resList[:topN]
    sel = thedf.iloc[rec]
    return theactive, rec, sel

df = pd.read_csv(readFile , sep='|' ,low_memory=False, index_col=None)

data =df[['id','name','culture_id','culture_name']]

grouping = data.groupby(by = 'culture_name')

rec = dict()
for key,values in grouping:
    item = pd.DataFrame(data=values.values,columns=list(data.columns))
    model = MakeSim(item)
    rec[key] = dict()
    rec[key]["target"] = item
    rec[key]["tfidMatrix"] = model[0]
    rec[key]["pdMatrix"] = model[1]
    rec[key]["sim"] = model[2]
    rec[key]["df"] = model[3]

thedf = rec[culture_name]["target"]
dataGroup = rec[culture_name]["df"]


#transform active with food_id to index
active = df[df['id'] == active].index[0]

print(active)

theactive, result, sel = RecFoodCulture(thedf, active, culture_name, dataGroup, topN)

#Result: แนะนำ
result = [str(r) for r in result]
print('|'.join(result))