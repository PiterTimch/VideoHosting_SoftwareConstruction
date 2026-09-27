import React from 'react';

interface TabButtonProps {
  label: string;
  isActive: boolean;
  onClick: () => void;
  className?: string;
}

export const TabButton: React.FC<TabButtonProps> = ({
  label,
  isActive,
  onClick,
  className = '',
}) => {
  return (
    <button
      onClick={onClick}
      className={`custom-tab-btn cursor-pointer ${
        isActive
          ? 'bg-[#FF2D7A] text-white shadow-[0_4px_12px_rgba(255,45,122,0.3)]'
          : 'bg-[#27272A] text-white/80 hover:bg-[#3F3F46] hover:text-white'
      } ${className}`}
    >
      {label}
    </button>
  );
};

interface TabButtonsProps {
  tabList: string[];
  activeTab: string;
  onTabChange: (tab: string) => void;
  className?: string;
}

export const TabButtons: React.FC<TabButtonsProps> = ({
  tabList,
  activeTab,
  onTabChange,
  className = '',
}) => {
  return (
    <div className={`flex items-center gap-2 overflow-x-auto no-scrollbar py-1 ${className}`}>
      {tabList.map((tab) => (
        <TabButton
          key={tab}
          label={tab}
          isActive={activeTab === tab}
          onClick={() => onTabChange(tab)}
        />
      ))}
    </div>
  );
};
