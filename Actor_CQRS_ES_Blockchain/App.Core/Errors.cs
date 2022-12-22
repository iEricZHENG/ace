namespace App.Core
{
    public class Errors
    {
        #region 账户相关错误
        /// <summary>
        /// 余额不足
        /// </summary>
        public static readonly int LackOfBalance = 101;
        /// <summary>
        /// 账户已被锁定
        /// </summary>
        public static readonly int AccountLocked = 102;
        /// <summary>
        /// 账户不可用
        /// </summary>
        public static readonly int AccountDisabled = 103;
        /// <summary>
        /// 币种账户已经存在，不需要开通
        /// </summary>
        public static readonly int AccountExist = 104;
        /// <summary>
        /// 当前币种账户未开通
        /// </summary>
        public static readonly int AccountNotExist = 105;
        /// <summary>
        /// 当前币种不是虚拟币
        /// </summary>
        public static readonly int CurrencyIsNotVirtualCoin = 106;
        /// <summary>
        /// 解锁需要补偿充值失败金额，但余额不足
        /// </summary>
        public static readonly int UnlockLackOfDepositCompensation = 107;
        #endregion      

        #region 资金账户错误
        /// <summary>
        /// 资金账号不可用
        /// </summary>
        public static readonly int CapitalAccountDisabled = 111;
        /// <summary>
        /// 资金账号余额不足
        /// </summary>
        public static readonly int CapitalAccountLackOfBalance = 112;
        /// <summary>
        /// 接受账户不能为空
        /// </summary>
        public static readonly int CapitalAcceptAccountNumberNotEmpty = 113;
        /// <summary>
        /// 来源账户不能为空
        /// </summary>
        public static readonly int CapitalFromAccountNumberNotEmpty = 114;
        /// <summary>
        /// 资金转账备注不能为空
        /// </summary>
        public static readonly int CapitalTransferRemarkNotEmpty = 115;
        #endregion

        #region 提现相关错误
        /// <summary>
        /// 提现金额太小
        /// </summary>
        public static readonly int WithdrawAmountTooSmall = 121;
        /// <summary>
        /// 提现金额超过单次限额
        /// </summary>
        public static readonly int WithdrawOverOnceLimit = 122;
        /// <summary>
        /// 任务领取后不能更换操作员
        /// </summary>
        public static readonly int ForbitToChangeOperator = 123;
        /// <summary>
        /// 人工驳回提现必须提供原因
        /// </summary>
        public static readonly int WithdrawRollbackLessOfReason = 124;
        /// <summary>
        /// 人工取消提现必须提供原因
        /// </summary>
        public static readonly int WithdrawCancelLessOfReason = 125;
        /// <summary>
        /// 绑定银行卡不存在
        /// </summary>
        public static readonly int UnBindBankCard = 126;
        /// <summary>
        /// 非处理中状态不能重置提现
        /// </summary>
        public static readonly int ForbitToResetWithdraw = 127;
        /// <summary>
        /// 非完成状态的提现不能驳回
        /// </summary>
        public static readonly int NonCompletedWithdrawForbitToRollback = 128;
        /// <summary>
        /// 提现在处理中，无法撤销！
        /// </summary>
        public static readonly int WithdrawForbitToRepeal = 129;
        #endregion

        #region 充值相关错误
        /// <summary>
        /// 客服人为取消充值必须提供原因
        /// </summary>
        public static readonly int DepositManualCancalLessOfReason = 131;
        /// <summary>
        /// 非法使用者
        /// </summary>
        public static readonly int InvalidDepositCode = 132;
        /// <summary>
        /// 密码不正确
        /// </summary>
        public static readonly int PasswordError = 133;
        /// <summary>
        /// 充值没有完成
        /// </summary>
        public static readonly int DepositUnComplete = 134;
        /// <summary>
        /// 充值码还未使用
        /// </summary>
        public static readonly int DepositCodeUnUsed = 135;
        /// <summary>
        /// 当前币种无法抵押快速充值
        /// </summary>
        public static readonly int CurrencyNotMortgage = 136;
        /// <summary>
        /// 当前币种的市场不存在，无法抵押
        /// </summary>
        public static readonly int CurrencyMarketNotExists = 137;
        /// <summary>
        /// 充值已经取消，无法重置
        /// </summary>
        public static readonly int ForbidResetCanceledDeposit = 138;
        /// <summary>
        /// 充值金额太小
        /// </summary>
        public static readonly int DepositAmountTooSmall = 139;
        /// <summary>
        /// 充值额度不足
        /// </summary>
        public static readonly int DepositLimitLack = 140;
        #endregion

        #region 下单相关错误
        /// <summary>
        /// 订单数量非法
        /// </summary>
        public const int UnlawfulnessOrderNumber = 141;
        /// <summary>
        /// 卖出数量超过余额
        /// </summary>
        public static readonly int VolumeOverOfBalance = 142;
        /// <summary>
        /// 订单与市场不匹配
        /// </summary>
        public static readonly int OrderTypeError = 143;
        /// <summary>
        /// 市场已经关闭
        /// </summary>
        public static readonly int MarketClosed = 144;
        /// <summary>
        /// 订单价格不合法
        /// </summary>
        public const int OrderPriceIllegal = 147;
        /// <summary>
        /// 市场不存在
        /// </summary>
        public const int MarketNotExits = 149;
        #endregion

        #region 虚拟币提现相关错误
        /// <summary>
        /// 提现状态不正确，可能存在并发导致
        /// </summary>
        public static readonly int WithdrawStatusError = 151;
        /// <summary>
        /// 提现不能取消
        /// </summary>
        public static readonly int WithdrawCannotCancel = 152;
        /// <summary>
        /// 提现银行卡已经添加
        /// </summary>
        public const int BankCardExists = 153;
        /// <summary>
        /// 虚拟币地址已经添加
        /// </summary>
        public const int CoinAddressExists = 154;
        /// <summary>
        /// 当前提现不允许重试
        /// </summary>
        public static readonly int WithdrawNotAllowRetry = 155;
        #endregion

        #region 币种相关错误
        /// <summary>
        /// 币种选择错误
        /// </summary>
        public const int CurrencyChooseError = 161;
        /// <summary>
        /// 精度错误
        /// </summary>
        public const int PrecisionError = 162;
        #endregion

        #region 邮箱手机绑定错误
        public const int CodeError = 201;

        /// <summary>
        /// 验证码过期
        /// </summary>
        public const int CodeTimeout = 202;
        /// <summary>
        /// 间隔时间不够，常用于发送验证码和邮箱内容
        /// </summary>
        public const int IntervalTimeNotEnough = 203;
        #endregion

        #region 注册返回结果
        /// <summary>
        /// 账号已经被使用
        /// </summary>
        public const int AccountRegistered = 163;//TODO
        public const int AccountUsed = 164;
        #endregion

        #region 资金到账相关错误
        /// <summary>
        /// 资金来源记录已经被使用过了
        /// </summary>
        public static readonly int FundSourceDisabled = 171;
        /// <summary>
        /// 资金来源记录与充值金额不匹配
        /// </summary>
        public static readonly int FundSourceDepositNotMatched = 172;
        #endregion

        #region 额外收支转账相关错误
        /// <summary>
        /// {0}不能为空
        /// </summary>
        public static readonly int TransferNotEmpty = 181;
        #endregion

        /// <summary>
        /// 验证等级太低，无法当前操作
        /// </summary>
        public static readonly int VerifyLevelTooLow = 204;
        /// <summary>
        /// 认证信息重复提交
        /// </summary>
        public const int CertificationRepeatSubmit = 205;
        /// <summary>
        /// 认证信息不存在
        /// </summary>
        public const int NoCertificationInfo = 206;
        /// <summary>
        /// 身份认证信息已经审核过了
        /// </summary>
        public const int CertificationAudited = 207;
        /// <summary>
        /// 被锁定
        /// </summary>
        public const int IsLocked = 208;
        /// <summary>
        /// 手机无法解绑，因为该账号是以手机注册的
        /// </summary>
        public const int CanNotUnbunding = 209;
        /// <summary>
        /// 不是用户自己锁定的账号，不能进行普通解锁
        /// </summary>
        public const int NotUserSelfLockCanNotNormalUnlock = 210;
        /// <summary>
        /// 密码已经设置
        /// </summary>
        public const int TradePasswordAlreadySet = 211;
        /// <summary>
        /// 提现被锁定24小时(用在忘记交易密码后)
        /// </summary>
        public const int WithdrawalLocked24H = 212;
        /// <summary>
        /// 交易密码与登录密码相等
        /// </summary>
        public const int TradePasswordEqualLoginPassword = 213;
        #region Common
        //不存在
        public const int EmailOrPhoneError = 1001;
        /// <summary>
        /// 交易密码错误
        /// </summary>
        public const int TradepasswordError = 1002;
        /// <summary>
        /// 值不能为空
        /// </summary>
        public const int ValueCanNotBeEmpty = 1003;
        /// <summary>
        /// 订单价格超过限制限价
        /// </summary>
        public const int OrderPriceMoreThanLimit = 1004;
        /// <summary>
        /// 价格超过精度限制
        /// </summary>
        public const int PricePrecisionLimit = 1005;
        /// <summary>
        /// 订单金额小于市场最小下单金额限制
        /// </summary>
        public const int OrderAmountLimit = 1006;
        /// <summary>
        /// 订单挂单数量精度超过了市场限制
        /// </summary>
        public const int OrderVolumeMoreThanLimit = 1007;
        /// <summary>
        /// 订单价格超过市场涨跌幅限制
        /// </summary>
        public const int OrderPriceMoreThanMarketPriceLimit = 1008;
        #endregion
        #region CMS
        /// <summary>
        /// 已经参与投票，不能再次投票
        /// </summary>
        public const int HaveToNote = 300;
        #endregion
        #region 账号安全
        /// <summary>
        /// Otp绑定密钥失效
        /// </summary>
        public const int OtpSecretTimeout = 401;
        /// <summary>
        /// 手机未绑定
        /// </summary>
        public const int PhoneNotBind = 402;
        /// <summary>
        /// 邮箱未绑定
        /// </summary>
        public const int EmailNotBind = 403;
        /// <summary>
        /// 手机已绑定，不能再次绑定
        /// </summary>
        public const int PhoneIsBinded = 404;
        /// <summary>
        /// 邮箱已经绑定，不能再次绑定
        /// </summary>
        public const int EmailIsBinded = 405;
        /// <summary>
        /// 身份证验证错误
        /// </summary>
        public const int IDNoVerifyError = 406;
        /// <summary>
        /// 超过最大错误次数(适用于密码类)
        /// </summary>
        public const int MoreThanMaxErros = 407;
        #endregion
        #region 个人信息
        /// <summary>
        /// 昵称已被使用
        /// </summary>
        public const int NickNameIsUsed = 501;
        /// <summary>
        /// 开放Key超过最大量限制
        /// </summary>
        public const int OpenKeyMoreThanMaxLimit = 502;
        #endregion
    }
}
