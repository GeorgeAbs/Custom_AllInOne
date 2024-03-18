using Llama;
using Plaice;
using LMForeignCalc;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Custom_AllInOne
{
    public class ManualValvesLNG : ILMForeignCalc
    {
        public bool ValidateProperty(bool blnIsUICall, LMADataSource DataSource, _LMAItems Items, string strPropertyName, object varValue)
        {
            if (varValue.ToString() != "MV") { return true; }

            LMPipingComp comp = DataSource.GetPipingComp(Items.Nth[1].Id);

            var plantObjectId = comp.InlineComps.Nth[1].PipeRunObject.PlantGroupObject.Id;

            var plantPath = DbInfoRetriever.GetPlantPath(DataSource, plantObjectId);


            string[] splittedPath = plantPath.Split('\\');
            if (splittedPath.Length > 2)
            {
                var unitPart = splittedPath[1];
                var unitNum = splittedPath[2].Split('-')[0].Trim();

                string type = comp.Attributes["PipingCompSubclass"].get_Value();
                /*if (type.ToLower().Contains("blockvalve") ||
                    type.ToLower().Contains("block valve") ||
                    type.ToLower().Contains("gate valve") ||
                    type.ToLower().Contains("cock valve") ||
                    type.ToLower().Contains("ball valve") ||
                    type.ToLower().Contains("hand control butterfly valve") ||
                    type.ToLower().Contains("hand valve") ||
                    type.ToLower().Contains("vent valve") ||
                    type.ToLower().Contains("globe valve") ||
                    type.ToLower().Contains("hand control valve"))*/
                if (type.ToLower().Contains("valve"))
                {
                    string valveId = Items.Nth[1].Id;
                    string tagSeqNo = DbInfoRetriever.GetNextTagSeqNo(DataSource, "\\" + unitPart + "\\" + unitNum, DbInfoRetriever.GetConnectionString(DataSource).Item1, valveId);


                    if (tagSeqNo != "")
                    {
                        if (tagSeqNo.Length == 1)
                        {
                            tagSeqNo = "000" + tagSeqNo;
                        }
                        else if (tagSeqNo.Length == 2)
                        {
                            tagSeqNo = "00" + tagSeqNo;
                        }
                        else if (tagSeqNo.Length == 3)
                        {
                            tagSeqNo = "0" + tagSeqNo;
                        }
                        string itemTag = unitPart + "-" + unitNum + "MV" + tagSeqNo;
                        comp.Attributes["ItemTag"].set_Value(itemTag);
                        comp.Attributes["TagPrefix"].set_Value("MV");
                        comp.Attributes["TagSequenceNo"].set_Value(tagSeqNo);
                    }
                    

                    Placement placement = new Placement();
                    if (comp.Representations.Nth[1].LabelPersists.Count == 0)
                    {
                        var symbol = DataSource.GetSymbol(comp.Representations.Nth[1].Id);
                        double x = double.Parse(symbol.Attributes["XCoordinate"].get_Value().ToString(), System.Globalization.CultureInfo.InvariantCulture);
                        double y = double.Parse(symbol.Attributes["YCoordinate"].get_Value().ToString(), System.Globalization.CultureInfo.InvariantCulture);

                        placement.PIDPlaceLabel("\\_New_catalog\\Piping\\Labels - Piping Components\\MV TAG GOST 2 mm.sym", new double[] { y,x,y,x,x,y}, comp.Representations.Nth[1]);
                    }
                } 
            }
            return true;
        }

        #region ILMForeignCalc implementations

        public bool DoCalculate(ref LMADataSource DataSource, ref _LMAItems Items, ref string PropertyName, ref object Value)
        {
            return true;
        }

        public bool DoValidateItem(ref LMADataSource DataSource, ref _LMAItems Items, ref ENUM_LMAValidateContext Context)
        {
            
            return true;
        }

        public bool DoValidateProperty(ref LMADataSource DataSource, ref _LMAItems Items, ref string PropertyName, ref object Value)
        {
            return ValidateProperty(true, DataSource, Items, PropertyName, Value);
        }

        public void DoValidatePropertyNoUI(ref LMADataSource DataSource, ref _LMAItems Items, ref string PropertyName, ref object Value)
        {
            ValidateProperty(false, DataSource, Items, PropertyName, Value);
        }

        #endregion
    }

    public class IsHeatingInstrLNG : ILMForeignCalc
    {
        public bool ValidateProperty(bool blnIsUICall, LMADataSource DataSource, _LMAItems Items, string strPropertyName, object varValue)
        {
            LMPipeRun run = DataSource.GetPipeRun(Items.Nth[1].Id);
            var instrs = run.Instruments;
            if (varValue.ToString() == "N")
            {
                foreach (LMInstrument instr in instrs)
                {
                    instr.Attributes["IsLineHeated"].set_Value("нет");
                    instr.Commit();
                }
            }

            else
            {
                foreach (LMInstrument instr in instrs)
                {
                    instr.Attributes["IsLineHeated"].set_Value("да");
                    instr.Commit();
                }
            }



            return true;
        }

        #region ILMForeignCalc implementations

        public bool DoCalculate(ref LMADataSource DataSource, ref _LMAItems Items, ref string PropertyName, ref object Value)
        {
            return true;
        }

        public bool DoValidateItem(ref LMADataSource DataSource, ref _LMAItems Items, ref ENUM_LMAValidateContext Context)
        {

            return true;
        }

        public bool DoValidateProperty(ref LMADataSource DataSource, ref _LMAItems Items, ref string PropertyName, ref object Value)
        {
            return ValidateProperty(true, DataSource, Items, PropertyName, Value);
        }

        public void DoValidatePropertyNoUI(ref LMADataSource DataSource, ref _LMAItems Items, ref string PropertyName, ref object Value)
        {
            ValidateProperty(false, DataSource, Items, PropertyName, Value);
        }

        #endregion
    }

    public class InstrumentsLNG : ILMForeignCalc
    {
        public bool ValidateProperty(bool blnIsUICall, LMADataSource DataSource, _LMAItems Items, string strPropertyName, object varValue)
        {
            LMInstrument instr = DataSource.GetInstrument(Items.Nth[1].Id);
            var plantObjectId = instr.PlantGroupObject.Id;
            
            var plantPath = DbInfoRetriever.GetPlantPath(DataSource, plantObjectId);
            var unitPart = "";
            var unitNum = "";
            

            string[] splittedPath = plantPath.Split('\\');
            if (splittedPath.Length > 2)
            {
                unitPart = splittedPath[1];
                unitNum = splittedPath[2].Split('-')[0].Trim();
            }

            string measureVal = instr.Attributes["MeasuredVariableCode"].get_Value().ToString();
            string modifier = instr.Attributes["InstrumentTypeModifier"].get_Value().ToString();
            string tagSeqNo = instr.Attributes["TagSequenceNo"].get_Value().ToString();
            string tagSuffix = instr.Attributes["TagSuffix"].get_Value().ToString();

            if (strPropertyName == "MeasuredVariableCode") measureVal = (string)varValue;
            else if (strPropertyName == "InstrumentTypeModifier") modifier = (string)varValue;
            else if (strPropertyName == "TagSequenceNo") tagSeqNo = (string)varValue;
            else if (strPropertyName == "TagSuffix") tagSuffix = (string)varValue;

            string itemTag = unitPart  + unitNum + measureVal + modifier + tagSeqNo + tagSuffix;
            var instrFunction = measureVal + modifier;

            instr.Attributes["ItemTag"].set_Value(itemTag);
            #region SetSetPointAlarmToEmpty
            try
            {
                string AlarmH = instr.Attributes["SetPointAlarmH"].get_Value().ToString();
                string AlarmHH = instr.Attributes["SetPointAlarmHH"].get_Value().ToString();
                string AlarmL = instr.Attributes["SetPointAlarmL"].get_Value().ToString();
                string AlarmLL = instr.Attributes["SetPointAlarmLL"].get_Value().ToString();

                if (AlarmH == "987.65") instr.Attributes["SetPointAlarmH"].set_Value("");
                if (AlarmHH == "987.65") instr.Attributes["SetPointAlarmHH"].set_Value("");
                if (AlarmL == "987.65") instr.Attributes["SetPointAlarmL"].set_Value("");
                if (AlarmLL == "987.65") instr.Attributes["SetPointAlarmLL"].set_Value("");
                
            } catch { }
            #endregion

            #region Heating By HeatTraceMedium
            if (instrFunction == "PT" || instrFunction == "PDT" || instrFunction == "LT" || instrFunction == "FT")
            {
                if (unitNum == "00" ||
                    unitNum == "05" ||
                    unitNum == "06" ||
                    unitNum == "12" ||
                    unitNum == "23" ||
                    unitNum == "47" ||
                    unitNum == "48" ||
                    unitNum == "83" ||
                    unitNum == "87" ||
                    unitNum == "90" ||
                    unitNum == "91" ||
                    unitNum == "97")
                {
                    instr.Attributes["HTraceMedium"].set_Value("E");
                }
                else
                {
                    instr.Attributes["HTraceMedium"].set_Value("N");
                }
            }

            else
            {
                instr.Attributes["HTraceMedium"].set_Value("N");
            }
            #endregion

            #region SignalType
            string signalTypeSpecified = "-";
            switch(instrFunction)
            {
                case "GT":
                    signalTypeSpecified = "AI";
                    break;

                case "FT":
                    signalTypeSpecified = "AI";
                    break;

                case "LT":
                    signalTypeSpecified = "AI";
                    break;

                case "LS":
                    signalTypeSpecified = "Namur";
                    break;

                case "PT":
                    signalTypeSpecified = "AI";
                    break;

                case "PDT":
                    signalTypeSpecified = "AI";
                    break;

                case "HS":
                    signalTypeSpecified = "DI";
                    break;

                case "TT":
                    signalTypeSpecified = "Pt100";
                    break;

                case "SDN":
                    signalTypeSpecified = "DO";
                    break;

                case "SDZS":
                    signalTypeSpecified = "Namur";
                    break;

                case "LZT":
                    signalTypeSpecified = "AI";
                    break;
            }
            instr.Attributes["SignalTypeSpecified"].set_Value(signalTypeSpecified);
            #endregion

            #region instrTypeSpecified
            string instrTypeSpecified = "";
            switch (instrFunction)
            {
                case "GT":
                    instrTypeSpecified = "Датчик контроля загазованности";
                    break;

                case "LT":
                    instrTypeSpecified = "Преобразователь уровня";
                    break;

                case "LS":
                    instrTypeSpecified = "Сигнализатор уровня";
                    break;

                case "PG":
                    instrTypeSpecified = "Манометр";
                    break;

                case "PT":
                    instrTypeSpecified = "Преобразователь давления";
                    break;

                case "PDT":
                    instrTypeSpecified = "Преобразователь перепада давления";
                    break;

                case "HS":
                    instrTypeSpecified = "Кнопочный пост";
                    break;

                case "TT":
                    instrTypeSpecified = "Термометр сопротивления";
                    break;

                case "TG":
                    instrTypeSpecified = "Термометр биметаллический";
                    break;

                case "PDG":
                    instrTypeSpecified = "Дифманометр";
                    break;

                case "SDV":
                    instrTypeSpecified = "Отсечной клапан";
                    break;

                case "SDN":
                    instrTypeSpecified = "Соленоид";
                    break;

                case "SDZS":
                    instrTypeSpecified = "Концевой выключатель (положение \"открыто/закрыто\")";
                    break;

                case "LV":
                    instrTypeSpecified = "Регулирующий клапан";
                    break;

                case "LZT":
                    instrTypeSpecified = "Датчик положения";
                    break;

                case "FG":
                    instrTypeSpecified = "Ротаметр";
                    break;

                case "FE":
                    var flowMeterType = instr.Attributes["InstrumentType"].get_Value().ToString();
                    switch(flowMeterType)
                    {
                        case "Orif plate & flanges":
                            instrTypeSpecified = "Фланцевая диафрагма";
                            break;

                        case "Venturi tube":
                            instrTypeSpecified = "Труба Вентури (первичный)";
                            break;

                        case "Vortex flow instr":
                            instrTypeSpecified = "Вихревой расходомер (первичный)";
                            break;

                        case "General inline element":
                            var reprName = instr.Representations.Nth[1].Attributes["FileName"].get_Value().ToString();
                            if (reprName.ToLower().Contains("generic"))
                            {
                                instrTypeSpecified = "Электромагнитный расходомер (преобразователь)";
                            }

                            else if (reprName.ToLower().Contains("coriolis"))
                            {
                                instrTypeSpecified = "Кориолисовый расходомер (преобразователь)";
                            }
                            break;

                        case "Inline averaging pitot tube":
                            instrTypeSpecified = "Трубка Пито (первичный)";
                            break;

                        case "Inline ultrasonic flow instr":
                            instrTypeSpecified = "Ультразвуковой расходомер (первичный)";
                            break;
                    }
                    break;

                case "FT":
                    var rels = instr.Representations.Nth[1].Relation2Relationships; //.Nth[1].Item1RepresentationObject.ModelItemObject.Attributes["ItemTypeName"].get_Value();
                    for (int i = 1; i <= rels.Count; i++)
                    {
                        var modelItem = rels.Nth[i].Item1RepresentationObject.ModelItemObject;
                        var itemType = modelItem.Attributes["ItemTypeName"].get_Value().ToString();
                        if (itemType == "PipeRun")
                        {
                            string textToWrite = "";
                            LMPipeRun run = DataSource.GetPipeRun(modelItem.Id);
                            var runType = run.Attributes["PipeRunType"].get_Value().ToString();
                            if (runType == "Conn to process/supply")
                            {
                                var runRepres =  run.Representations;
                                for(int iRunRepres = 1; iRunRepres <= runRepres.Count; iRunRepres++)
                                {
                                    string runRersType = run.Representations.Nth[iRunRepres].Attributes["RepresentationType"].get_Value().ToString();

                                    if (runRersType.ToLower().Contains("connector"))
                                    {
                                        LMConnector connector = DataSource.GetConnector(run.Representations.Nth[iRunRepres].Id);
                                        var firstSide = connector.ConnectItem1SymbolObject;
                                        var secondSide = connector.ConnectItem2SymbolObject;
                                        if (firstSide != null)
                                        {
                                            if (firstSide.ModelItemObject.Attributes["ItemTypeName"].get_Value().ToString() == "Instrument")
                                            {
                                                LMInstrument instrument = DataSource.GetInstrument(firstSide.ModelItemObject.Id);
                                                var firstFlowMeterType = instrument.Attributes["InstrumentType"].get_Value().ToString();

                                                switch (firstFlowMeterType)
                                                {
                                                    case "Orif plate & flanges":
                                                        instrTypeSpecified = "Преобразователь перепада давления";
                                                        break;

                                                    case "Venturi tube":
                                                        instrTypeSpecified = "Преобразователь перепада давления";
                                                        break;

                                                    case "Vortex flow instr":
                                                        instrTypeSpecified = "Вихревой расходомер (преобразователь)";
                                                        break;

                                                    case "General inline element":
                                                        var reprName = instr.Representations.Nth[1].Attributes["FileName"].get_Value().ToString();
                                                        if (reprName.ToLower().Contains("generic"))
                                                        {
                                                            instrTypeSpecified = "Электромагнитный расходомер (преобразователь)";
                                                        }

                                                        else if (reprName.ToLower().Contains("coriolis"))
                                                        {
                                                            instrTypeSpecified = "Кориолисовый расходомер (преобразователь)";
                                                        }
                                                        break;

                                                    case "Inline averaging pitot tube":
                                                        instrTypeSpecified = "Преобразователь перепада давления";
                                                        break;

                                                    case "Inline ultrasonic flow instr":
                                                        instrTypeSpecified = "Ультразвуковой расходомер (преобразователь)";
                                                        break;

                                                    case "Var area flow instr":
                                                        instrTypeSpecified = "Ротаметр";
                                                        break;
                                                }
                                            }
                                        }
                                        if (secondSide != null)
                                        {
                                            if (secondSide.ModelItemObject.Attributes["ItemTypeName"].get_Value().ToString() == "Instrument")
                                            {
                                                LMInstrument instrument = DataSource.GetInstrument(secondSide.ModelItemObject.Id);
                                                var secondFlowMeterType = instrument.Attributes["InstrumentType"].get_Value().ToString();

                                                switch (secondFlowMeterType)
                                                {
                                                    case "Orif plate & flanges":
                                                        instrTypeSpecified = "Преобразователь перепада давления";
                                                        break;

                                                    case "Venturi tube":
                                                        instrTypeSpecified = "Преобразователь перепада давления";
                                                        break;

                                                    case "Vortex flow instr":
                                                        instrTypeSpecified = "Вихревой расходомер (преобразователь)";
                                                        break;

                                                    case "General inline element":
                                                        var reprName = instr.Representations.Nth[1].Attributes["FileName"].get_Value().ToString();
                                                        if (reprName.ToLower().Contains("generic"))
                                                        {
                                                            instrTypeSpecified = "Электромагнитный расходомер (преобразователь)";
                                                        }

                                                        else if (reprName.ToLower().Contains("coriolis"))
                                                        {
                                                            instrTypeSpecified = "Кориолисовый расходомер (преобразователь)";
                                                        }
                                                        break;

                                                    case "Inline averaging pitot tube":
                                                        instrTypeSpecified = "Преобразователь перепада давления";
                                                        break;

                                                    case "Inline ultrasonic flow instr":
                                                        instrTypeSpecified = "Ультразвуковой расходомер (преобразователь)";
                                                        break;

                                                    case "Var area flow instr":
                                                        instrTypeSpecified = "Ротаметр";
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                    
                                }

                                
                                break;
                            }
                        }
                    }
                   
                    break;

            }
            instr.Attributes["InstrTypeSpecified"].set_Value(instrTypeSpecified);
            #endregion
            return true;
        }

        #region ILMForeignCalc implementations

        public bool DoCalculate(ref LMADataSource DataSource, ref _LMAItems Items, ref string PropertyName, ref object Value)
        {
            return true;
        }

        public bool DoValidateItem(ref LMADataSource DataSource, ref _LMAItems Items, ref ENUM_LMAValidateContext Context)
        {

            return true;
        }

        public bool DoValidateProperty(ref LMADataSource DataSource, ref _LMAItems Items, ref string PropertyName, ref object Value)
        {
            return ValidateProperty(true, DataSource, Items, PropertyName, Value);
        }

        public void DoValidatePropertyNoUI(ref LMADataSource DataSource, ref _LMAItems Items, ref string PropertyName, ref object Value)
        {
            ValidateProperty(false, DataSource, Items, PropertyName, Value);
        }

        #endregion
    }

    public class SmartOpcLNG : ILMForeignCalc
    {

        private bool ValidateProperty(bool blnIsUICall, LMADataSource DataSource, _LMAItems Items, string strPropertyName, object varValue)
        {
            LMOPC opc = DataSource.GetOPC(Items.Nth[1].Id);
            var symbol = DataSource.GetSymbol(opc.Representations.Nth[1].Id);
            string opcSymbolPath = symbol.Attributes["FileName"].get_Value().ToString();//Off Drawing Instrument Connector
            string[] splittedSignal = varValue.ToString().Split(':');
            if (splittedSignal.Count() == 2)
            {
                if (opc.Attributes["OPCType"].get_Value().ToString() == "Off Drawing Instrument Connector")
                {
                    string opcText = varValue.ToString();
                    var pairedOPC = opc.pairedWithOPCObject;
                    DataSource.BeginTransaction();

                    if (opcSymbolPath.ToLower().Contains("_in_"))
                    {
                        pairedOPC.Attributes["ToFromText"].set_Value("C : " + splittedSignal[1].Trim());
                    }
                    else if (opcSymbolPath.ToLower().Contains("_out_"))
                    {
                        pairedOPC.Attributes["ToFromText"].set_Value("E : " + splittedSignal[1].Trim());
                    }

                    pairedOPC.Commit();
                    DataSource.CommitTransaction();
                    pairedOPC.Commit();
                }
            }
            
            return true;
        }

        #region ILMForeignCalc implementations

        public bool DoCalculate(ref LMADataSource DataSource, ref _LMAItems Items, ref string PropertyName, ref object Value)
        {
            return true;
        }

        public bool DoValidateItem(ref LMADataSource DataSource, ref _LMAItems Items, ref ENUM_LMAValidateContext Context)
        {
            return true;
        }

        public bool DoValidateProperty(ref LMADataSource DataSource, ref _LMAItems Items, ref string PropertyName, ref object Value)
        {
            return ValidateProperty(true, DataSource, Items, PropertyName, Value);
        }

        public void DoValidatePropertyNoUI(ref LMADataSource DataSource, ref _LMAItems Items, ref string PropertyName, ref object Value)
        {
            ValidateProperty(false, DataSource, Items, PropertyName, Value);
        }

        #endregion
    }

    public class AutoMtbLNG : ILMForeignCalc
    {
        public bool ValidateProperty(bool blnIsUICall, LMADataSource DataSource, _LMAItems Items, string strPropertyName, object varValue)
        {
            string mtb = varValue.ToString();
            LMPipeRun run = DataSource.GetPipeRun(Items.Nth[1].Id);
            if (File.Exists("D:\\МТБ_СПГ\\МТБ.csv"))
            {
                List<string[]> mtbLines = new List<string[]>();
                string[] fullFile = File.ReadAllLines("D:\\МТБ_СПГ\\МТБ.csv", Encoding.UTF8);
                for (int i = 6; i < fullFile.Count(); i++)
                {
                    mtbLines.Add(fullFile[i].Split(';'));
                }
                
                
                var correctParams = mtbLines.Where(x => x[1] == mtb);
                if (correctParams.Any())
                {
                    string[] paramsReplaced = correctParams.First();

                    string gasFluid = paramsReplaced[2].Replace("Ж/Г", "Liquid/Gas").Replace("Ж", "Liquid").Replace("Г", "Gas");
                    string operNormPres = paramsReplaced[3].Replace(",", ".").Replace("-", "");
                    string operNormTemp = paramsReplaced[4].Replace(",", ".").Replace("-", "");
                    string operMaxPres = paramsReplaced[5].Replace(",", ".").Replace("-", "");
                    string operMaxTemp = paramsReplaced[6].Replace(",", ".").Replace("-", "");
                    string operMinPres = paramsReplaced[7].Replace(",", ".").Replace("-", "");
                    string operMinTemp = paramsReplaced[8].Replace(",", ".").Replace("-", "");
                    string operNormViscosityV = paramsReplaced[9].Replace(",", ".").Replace("-", "");
                    string operNormViscosityL = paramsReplaced[10].Replace(",", ".").Replace("-", "");
                    string operNormMassDens = paramsReplaced[11].Replace(",", ".").Replace("-", "");
                    string operMinMassDens = paramsReplaced[12].Replace(",", ".").Replace("-", "");
                    string operMaxMassDens = paramsReplaced[13].Replace(",", ".").Replace("-", "");
                    string operNormMolecularWeight = paramsReplaced[13].Replace(",", ".").Replace("-", "");

                    if (operNormPres != "")
                    {
                        operNormPres = (double.Parse(operNormPres, System.Globalization.CultureInfo.InvariantCulture) - 0.1).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    }

                    if (operMaxPres != "")
                    {
                        operMaxPres = (double.Parse(operMaxPres, System.Globalization.CultureInfo.InvariantCulture) - 0.1).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    }

                    if (operMinPres != "")
                    {
                        operMinPres = (double.Parse(operMinPres, System.Globalization.CultureInfo.InvariantCulture) - 0.1).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    }

                    run.Attributes["AggregateState"].set_Value(gasFluid);
                    run.Attributes["ProcessOperating.Norm.Pressure"].set_Value(operNormPres);
                    run.Attributes["ProcessOperating.Norm.Temperature"].set_Value(operNormTemp);
                    run.Attributes["ProcessOperating.Max.Pressure"].set_Value(operMaxPres);
                    run.Attributes["ProcessOperating.Max.Temperature"].set_Value(operMaxTemp);
                    run.Attributes["ProcessOperating.Min.Pressure"].set_Value(operMinPres);
                    run.Attributes["ProcessOperating.Min.Temperature"].set_Value(operMinTemp);
                    run.Attributes["OperNormViscosityL"].set_Value(operNormViscosityL);
                    run.Attributes["OperNormViscosityV"].set_Value(operNormViscosityV);
                    run.Attributes["ProcessOperating.Norm.MassDensity"].set_Value(operNormMassDens);
                    run.Attributes["ProcessOperating.Min.MassDensity"].set_Value(operMinMassDens);
                    run.Attributes["ProcessOperating.Max.MassDensity"].set_Value(operMaxMassDens);
                    run.Attributes["ProcessOperating.Norm.MolecularWeight"].set_Value(operNormMolecularWeight);
                }
            }
            if (mtb == "-0000")
            {
                run.Attributes["AggregateState"].set_Value("");
                run.Attributes["ProcessOperating.Norm.Pressure"].set_Value("");
                run.Attributes["ProcessOperating.Norm.Temperature"].set_Value("");
                run.Attributes["ProcessOperating.Max.Pressure"].set_Value("");
                run.Attributes["ProcessOperating.Max.Temperature"].set_Value("");
                run.Attributes["ProcessOperating.Min.Pressure"].set_Value("");
                run.Attributes["ProcessOperating.Min.Temperature"].set_Value("");
                run.Attributes["OperNormViscosityL"].set_Value("");
                run.Attributes["OperNormViscosityV"].set_Value("");
                run.Attributes["ProcessOperating.Norm.MassDensity"].set_Value("");
                run.Attributes["ProcessOperating.Min.MassDensity"].set_Value("");
                run.Attributes["ProcessOperating.Max.MassDensity"].set_Value("");
                run.Attributes["ProcessOperating.Norm.MolecularWeight"].set_Value("");
            }
            //Items.Nth[1].Attributes[strPropertyName].set_Value(varValue);
            return true;
        }

        #region ILMForeignCalc implementations

        public bool DoCalculate(ref LMADataSource DataSource, ref _LMAItems Items, ref string PropertyName, ref object Value)
        {
            return true;
        }

        public bool DoValidateItem(ref LMADataSource DataSource, ref _LMAItems Items, ref ENUM_LMAValidateContext Context)
        {
            
            return true;
        }

        public bool DoValidateProperty(ref LMADataSource DataSource, ref _LMAItems Items, ref string PropertyName, ref object Value)
        {
            return ValidateProperty(true, DataSource, Items, PropertyName, Value);
        }

        public void DoValidatePropertyNoUI(ref LMADataSource DataSource, ref _LMAItems Items, ref string PropertyName, ref object Value)
        {
            ValidateProperty(false, DataSource, Items, PropertyName, Value);
        }

        #endregion
    }
}
