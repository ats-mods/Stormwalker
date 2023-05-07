using System;
using Eremite;
using Eremite.Buildings.UI.Trade;
using Eremite.Services;
using UnityEngine;

namespace Stormwalker {
    public static class TradePatches {

        public static void MatchOffer(TradingGoodSlot slot){
            var myPanel = GetGoodsPanel(slot);
            Plugin.Log($"{myPanel == null}, {slot}");
            if(myPanel == null) 
                return;
            var otherPanel = CounterpartOf(myPanel);
            var gap = GetOfferGap(slot, myPanel, otherPanel);
            var itemsNeeded = gap / GetValueOf(slot, myPanel);
            var itemsToOffer = (int) Math.Ceiling(itemsNeeded);
            // Plugin.Log($"{gap}, {itemsNeeded}, {itemsToOffer}, {myPanel.showSellValue}, {GetValueOf(slot, myPanel)}");
            if(!myPanel.showSellValue && itemsToOffer > itemsNeeded)
                itemsToOffer -= 1;

            var tradingGood = myPanel.Offer.goods[slot.GoodName];
            var bestAmountOption = Mathf.Clamp(itemsToOffer, 0, tradingGood.Sum);
            myPanel.SetNewOffer(tradingGood, bestAmountOption);
        }

        public static TradingGoodsPanel GetGoodsPanel(TradingGoodSlot slot){
            var ancestor = slot.transform.parent.parent.parent;
            var result = ancestor.GetComponent<TradingGoodsPanel>();
            if(result == null)  // The 'offered' slots are nested 1 level deeper in the hud
                result = ancestor.parent.GetComponent<TradingGoodsPanel>();
            return result;
        }

        private static TradingGoodsPanel CounterpartOf(TradingGoodsPanel panel){
            var go = panel.transform.parent;
            var otherGo = go.Find(panel.showSellValue? "TraderPanel" : "VillagePanel");
            return otherGo.GetComponent<TradingGoodsPanel>();
        }

        private static double GetOfferGap(TradingGoodSlot slot, TradingGoodsPanel myPanel, TradingGoodsPanel otherPanel){
            var currentGap = OfferPrice(otherPanel) - OfferPrice(myPanel);
            return Math.Round(currentGap + OfferPriceOfTradeGood(slot, myPanel), 2);
        }

        private static double OfferPrice(TradingGoodsPanel panel){
            var trade = GameMB.TradeService;
            return panel.showSellValue? trade.GetValueInCurrency(panel.Offer) : trade.GetBuyValueInCurrency(panel.Offer);
        }

        private static double OfferPriceOfTradeGood(TradingGoodSlot slot, TradingGoodsPanel myPanel){
            var trade = GameMB.TradeService as TradeService;
            var playerSells = myPanel.showSellValue;
            var tradingGood = myPanel.Offer.goods[slot.GoodName];
            return myPanel.showSellValue? trade.GetValueInCurrency(tradingGood) : trade.GetBuyValueInCurrency(tradingGood);
        }

        private static double GetValueOf(TradingGoodSlot slot, TradingGoodsPanel myPanel){
            var trade = GameMB.TradeService;
            return myPanel.showSellValue? trade.GetValueInCurrency(slot.GoodName, 1) : trade.GetBuyValueInCurrency(slot.GoodName, 1);
        }
    }
}