using ModelContextProtocol.Server;
using System.ComponentModel;

namespace SimpleMcpServer.Application.Prompts;

[McpServerPromptType]
public class StockAnalysisPrompts
{
    [McpServerPrompt(Name = "stock_overview")]
    [Description("Comprehensive overview of a Thai SET/mai stock — fundamentals, latest price, recent dividends, and company profile.")]
    public static string StockOverview(
        [Description("SET/mai stock symbol, e.g. PTT, KBANK, AOT.")] string symbol)
    {
        return $$"""
            ขอภาพรวมหุ้น {{symbol}} (Thai SET/mai) แบบครบถ้วน

            กรุณาเรียกใช้เครื่องมือต่อไปนี้และสรุปผลให้ผู้ใช้:
            1. fundamental_lookup(symbol="{{symbol}}") — ดู PE, PBV, ROE, ROA, EPS, market cap, งบดุลรอบล่าสุด
            2. daily_prices(symbol="{{symbol}}") — ราคาปิดล่าสุด, volume, value
            3. dividends(symbol="{{symbol}}", limit=5) — ประวัติเงินปันผล 5 ครั้งล่าสุด
            4. company_profile(symbol="{{symbol}}") — ลักษณะธุรกิจ, ที่อยู่, CG score, นโยบายปันผล

            Then synthesize into a single overview covering:
            - Business description (Thai or English, whichever is clearer)
            - Latest valuation snapshot (PE / PBV / dividend yield / market cap)
            - Profitability (ROE, ROA, net profit trend)
            - Recent price level vs fundamentals
            - Dividend track record summary
            - Notable governance signals (CG score, CAC flag) if available

            If any tool call returns "no data found", note it explicitly rather than fabricating.
            """;
    }

    [McpServerPrompt(Name = "financial_health_check")]
    [Description("Deep-dive financial health analysis for a Thai SET/mai stock for a specific quarter — pulls fundamentals, raw P&L/balance sheet lines, and dividend coverage.")]
    public static string FinancialHealthCheck(
        [Description("SET/mai stock symbol, e.g. PTT, KBANK.")] string symbol,
        [Description("Fiscal year, e.g. 2024.")] int fiscal,
        [Description("Quarter: '1', '2', '3' for quarterly, '9' for annual.")] string quarter)
    {
        return $$"""
            วิเคราะห์สุขภาพทางการเงินของ {{symbol}} สำหรับงวด fiscal {{fiscal}} ไตรมาส {{quarter}}

            Steps:
            1. fundamental_lookup(symbol="{{symbol}}", fiscal={{fiscal}}, quarter="{{quarter}}") — ดูอัตราส่วนหลัก
            2. financial_statements(symbol="{{symbol}}", fiscal={{fiscal}}, quarter="{{quarter}}", finStateType="C") — ดูงบเต็มฉบับ consolidated
            3. dividends(symbol="{{symbol}}", limit=4) — ดูปันผลรอบล่าสุดเพื่อประเมิน dividend coverage

            ในการวิเคราะห์ ให้ครอบคลุม:
            - **Profitability**: net profit, ROE, ROA, profit margin trend (compare net_profit_cons vs revenue from financial_statements)
            - **Solvency / Leverage**: debt_equity_ratio, gearing, total_asset vs total_equity vs liabilities
            - **Liquidity**: cash position relative to liabilities
            - **Valuation**: PE, PBV — เทียบกับ ROE / EPS growth
            - **Dividend sustainability**: เงินปันผลที่จ่าย vs net profit (payout ratio implied)
            - **Red flags**: หาก ROE สูงผิดปกติ, debt_equity_ratio พุ่งสูง, หรือ EPS ติดลบ ให้ระบุชัดเจน

            หากไม่มีข้อมูลในงวดที่ระบุ บอกผู้ใช้ว่าให้ลอง fiscal/quarter อื่น
            """;
    }

    [McpServerPrompt(Name = "dividend_history")]
    [Description("Summarize dividend history for a Thai SET/mai stock — payment dates, types (cash/stock), trend, and dividend yield.")]
    public static string DividendHistory(
        [Description("SET/mai stock symbol, e.g. PTT, KBANK.")] string symbol,
        [Description("How many recent dividend events to pull (1-100, default 20).")] int limit = 20)
    {
        return $$"""
            สรุปประวัติเงินปันผลของ {{symbol}} ย้อนหลัง {{limit}} ครั้งล่าสุด

            Steps:
            1. dividends(symbol="{{symbol}}", limit={{limit}}) — ดึง raw dividend events
            2. fundamental_lookup(symbol="{{symbol}}") — ดู dividend_yield ปัจจุบัน

            สรุปออกมาเป็น:
            - **Timeline table**: news_ann_date, payment_date, dividend_type (CD=cash, SD=stock), amount per share, source (1=กำไรสะสม, 2=กำไร, 3=ทั้งสอง)
            - **Total cash dividend per year** (group by year of payment_date for CD type only)
            - **Trend**: ปันผลเพิ่มขึ้น/ลดลง/คงที่ ในช่วงเวลาที่ดู
            - **Special dividends**: หาก dividend_flag เป็น @ หรือ & หรือ $ ให้แยกแสดง
            - **Stock dividends / splits**: หากมี SD ให้ระบุ ratio
            - **Current dividend yield** จาก fundamental_lookup
            - **Consistency score**: บริษัทจ่ายปันผลทุกปีไหม? เคยงดจ่ายในปีไหน?
            """;
    }

    [McpServerPrompt(Name = "compare_stocks")]
    [Description("Side-by-side comparison of multiple Thai SET/mai stocks — pulls fundamentals for each and contrasts valuation, profitability, and dividend yield.")]
    public static string CompareStocks(
        [Description("Comma-separated list of SET/mai symbols, e.g. \"PTT,KBANK,AOT,CPALL\". 2-5 symbols recommended.")] string symbols)
    {
        return $$"""
            เปรียบเทียบหุ้น Thai SET/mai ดังต่อไปนี้: {{symbols}}

            Steps:
            1. แยก symbols ออกจากกันด้วย comma
            2. สำหรับแต่ละ symbol ให้เรียก:
               - fundamental_lookup(symbol="<sym>") — ดู PE, PBV, ROE, ROA, EPS, dividend_yield, mk_cap
               - company_profile(symbol="<sym>") — เพื่อรู้ว่าทำธุรกิจอะไร
            3. หากตัวใดไม่มีข้อมูล ให้ระบุและข้ามไป

            สร้างตารางเปรียบเทียบที่มีคอลัมน์:
            | Symbol | Business | Market Cap | PE | PBV | ROE | ROA | Div Yield | EPS |

            แล้วเขียนสรุปประเด็น:
            - ตัวที่ valuation ถูก/แพงที่สุด (PE)
            - ตัวที่ profitability ดีที่สุด (ROE)
            - ตัวที่ให้ปันผลดีที่สุด (dividend yield)
            - ตัวใหญ่/เล็กตาม market cap
            - หากเป็นกลุ่มอุตสาหกรรมเดียวกันให้ระบุว่าตัวไหนน่าสนใจที่สุดและทำไม

            ตอบเป็นภาษาไทยเป็นหลัก โดยศัพท์เทคนิคทางการเงินใช้อังกฤษได้
            """;
    }
}
